using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
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
        private Type Middleware;

        public AkiraServerV4(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger<AkiraServerV4>();

            Settings = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server").Get<GeneralSettings>();

            SetMiddleware<BaseContext.BaseContext>();

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
                catch (SocketException e)
                {
                    Logger.LogError(e.ToString());
                }
            }

            TcpListener.Stop();
        }

        public void StopListening()
        {
            IsListening = false;
        }

        public void SetMiddleware<T>() where T : BaseContext.BaseContext
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
                        request = await Request.BuildRequest(netStream, Settings.RequestSettings).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }

                    if (request is not null && RequestedEndpoint(request) is ExecutedCommand executedCommand1)
                    {
                        executedCommand = executedCommand1;
                    }

                    using (BaseContext.BaseContext context = ContextBuilder.CreateContext(executedCommand?.ClassExecuted ?? Middleware, netStream, request, ServiceProvider))
                    {
                        context.Response.EnableCrossOriginRequests();

                        bool connectionAborted = false;



                        if (request is null)
                        {
                            context.Response.Body = await context.BadRequest(exception).ConfigureAwait(false);
                        }
                        else if (executedCommand is null)
                        {
                            context.Response.Body = await context.NotFound(request).ConfigureAwait(false);
                        }
                        else
                        {
                            try
                            {
                                context.Response.Body = await InvokeHandlerAsync(context, executedCommand).ConfigureAwait(false);
                            }
                            catch (IOException)
                            {
                                // Not really important
                                // It happens when the client force closes the connection
                                connectionAborted = true;
                            }
                            catch (Exception ex)
                            {
                                context.Response.Body = await context.InternalServerError(ex).ConfigureAwait(false);
                            }
                        }


                        if (!connectionAborted)
                        {
                            await context.WriteBodyAsync().ConfigureAwait(false);
                            await context.NetworkStream.FlushAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}