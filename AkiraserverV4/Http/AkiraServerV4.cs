using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private readonly ILogger<AkiraServerV4> Logger;

        public bool IsListening { get; private set; }

        private readonly TcpListener TcpListener;
        private readonly IServiceProvider ServiceProvider;
        private readonly GeneralSettings Settings;
        private Type Middleware { get; set; }

        public AkiraServerV4(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger<AkiraServerV4>();

            Settings = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server").Get<GeneralSettings>();

            SetMiddleware<BaseMiddleware>();

            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: Settings.Port);

            ReloadServerConfig();
        }

        public void ReloadServerConfig()
        {
            TcpListener.Server.ExclusiveAddressUse = Settings.ExclusiveAddressUse;
            TcpListener.Server.ReceiveTimeout = Settings.RequestSettings.ReciveTimeout;
            TcpListener.Server.SendTimeout = Settings.ResponseSettings.SendTimeout;
            TcpListener.Server.Ttl = Settings.Ttl;
            TcpListener.Server.UseOnlyOverlappedIO = Settings.UseOnlyOverlappedIO;
            //TcpListener.Server.Blocking = false;
        }

        public AkiraServerV4(ServiceProvider serviceProvider, Assembly assembly) : this(serviceProvider)
        {
            LoadRouting(assembly);
        }

        public async Task StartListening()
        {
            if (Endpoints is null)
            {
                Logger.LogError("There are no endpoints loaded".ToErrorString(this));
                return;
            }

            if (IsListening)
            {
                Logger.LogWarning("Already Listening".ToErrorString(this));
                return;
            }

            TcpListener.Start();
            IsListening = true;

            Logger.LogInformation($"Now Listening on '{TcpListener.LocalEndpoint}'...");

            while (IsListening)
            {
                try
                {
                    await RequestProcessing().ConfigureAwait(false);
                }
                catch (Exception e) when (e is SocketException || e is IOException)
                {
#if DEBUG
                    Logger.LogError(e.ToString());
#endif
                }
            }

            TcpListener.Stop();
        }

        public void StopListening()
        {
            IsListening = false;
        }

        public void SetMiddleware<T>() where T : BaseMiddleware
        {
            Middleware = typeof(T);
        }

        public async Task RequestProcessing()
        {
            //Listener.AcceptSocketAsync
            using (TcpClient client = await TcpListener.AcceptTcpClientAsync().ConfigureAwait(false))
            {
                Logger.LogInformation($"New connection from: {client.Client.RemoteEndPoint}");
                // Get a stream object for reading and writing
                using (NetworkStream netStream = client.GetStream())
                {

                    // Stream Checks =================================================================
                    if (!netStream.CanRead)
                    {
                        Logger.LogCritical("Can Not Read Stream".ToErrorString(this));
                        netStream.Close();
                        client.Close();
                        return;
                    }

                    if (!netStream.CanWrite)
                    {
                        Logger.LogCritical("Can Not Write To The Stream".ToErrorString(this));
                        netStream.Close();
                        client.Close();
                        return;
                    }

                    // Stream Checks =================================================================

                    ExecutedCommand executedCommand = null;
                    Request request = null;
                    Exception exception = null;
                    try
                    {
#if DEBUG
                        request = await Request.BuildRequest(netStream, Settings.RequestSettings, ServiceProvider.GetRequiredService<ILogger<Request>>()).ConfigureAwait(false);
                        //request.LogPacket();
#else
                        request = await Request.BuildRequest(netStream, Settings.RequestSettings).ConfigureAwait(false);
#endif
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }

                    if (request is not null && RequestedEndpoint(request) is ExecutedCommand executedCommand1)
                    {
                        executedCommand = executedCommand1;
                    }

                    var response = new Response(Settings.ResponseSettings);

                    var middleware = ContextBuilder.CreateContext(executedCommand?.ClassExecuted, Middleware, netStream, request, response, ServiceProvider);

                    object bodyContent;
                    bool isReturnTypeVoid = false;
                    if (executedCommand is null)
                    {
                        bodyContent = await middleware.NotFound(request).ConfigureAwait(false);
                        
                    }
                    else
                    {
                        try
                        {
                            var temp = await middleware.ActionExecuting(executedCommand).ConfigureAwait(false);
                            isReturnTypeVoid = temp.IsReturnTypeVoid;
                            bodyContent = temp.ReturnValue;
                        }
                        catch (MalformedRequestException MalformedRequestException)
                        {
                            bodyContent = await middleware.BadRequest(MalformedRequestException).ConfigureAwait(false);
                        }
                        catch (IOException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            bodyContent = await middleware.InternalServerError(ex).ConfigureAwait(false);
                        }
                    }

                    await middleware.Context.WriteBodyAsync(isReturnTypeVoid, bodyContent).ConfigureAwait(false);
                    await middleware.Context.NetworkStream.FlushAsync().ConfigureAwait(false);
                }
            }
        }
    }
}