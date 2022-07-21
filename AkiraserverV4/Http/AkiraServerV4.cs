using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
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
        private readonly GeneralSettings GeneralSettings;
        private BaseMiddleware Middleware { get; set; }

        public AkiraServerV4(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger<AkiraServerV4>();

            GeneralSettings = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server").Get<GeneralSettings>();

            LoadRouting(typeof(BaseContext).Assembly);

            SetMiddleware<BaseMiddleware>();

            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: GeneralSettings.Port);

            ReloadServerConfig();
        }

        public void ReloadServerConfig()
        {
            TcpListener.Server.ExclusiveAddressUse = GeneralSettings.ExclusiveAddressUse;
            TcpListener.Server.ReceiveTimeout = GeneralSettings.RequestSettings.ReciveTimeout;
            TcpListener.Server.SendTimeout = GeneralSettings.ResponseSettings.SendTimeout;
            TcpListener.Server.Ttl = GeneralSettings.Ttl;
            //TcpListener.Server.UseOnlyOverlappedIO = Settings.UseOnlyOverlappedIO;
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
                    await ServeNext().ConfigureAwait(false);
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

        public void SetMiddleware<T>() where T : BaseMiddleware, new()
        {
            Middleware = new T();
        }

        public void SetMiddleware<T>(T instance) where T : BaseMiddleware
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Middleware = instance;
        }

        public async Task ServeNext()
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

                    using var bufferedStream = new BufferedStream(netStream, GeneralSettings.BufferSize);

                    await ProcessRequest(bufferedStream);
                }
            }
        }

        public async Task ProcessRequest(BufferedStream NetworkStream)
        {
            var request = await Request.TryParseRequest(NetworkStream, GeneralSettings.RequestSettings).ConfigureAwait(false);

#if DEBUG
            request.LogPacket(ServiceProvider.GetRequiredService<ILogger<Request>>());
#endif

            ExecutedCommand executedCommand = null;

            var response = new Response(GeneralSettings.ResponseSettings, NetworkStream);

            if (request.ParseErrors.Count > 0)
            {
                executedCommand = GetEndpoint(SpecialEndpoint.BadRequest);

                response.HttpResponseHeaders.Status = HttpStatus.BadRequest;

                request.Params.Add("ParseErrors", request.ParseErrors);
            }

            if (executedCommand is null)
            {
                // parse went right, here we find what we should do
                executedCommand = GetEndpoint(request);
            }

            if (executedCommand is null)
            {
                // we didn't find what we should do
                executedCommand = GetEndpoint(SpecialEndpoint.NotFound);

                response.HttpResponseHeaders.Status = HttpStatus.NotFound;

                request.Params.Add(nameof(Request), request);
            }

            var context = ContextBuilder.CreateContext(executedCommand, request, response, ServiceProvider);

            try
            {
                var temp = await Middleware.ActionExecuting(context, request, executedCommand);

                if (response.HttpResponseHeaders.Status == HttpStatus.Unset)
                {
                    response.HttpResponseHeaders.Status = HttpStatus.Ok;
                }

                await response.WriteBodyAsync(temp);
            }
            catch (IOException)
            {
                // dropped connections and such i guess
            }
            catch (Exception ex)
            {
                executedCommand = GetEndpoint(SpecialEndpoint.InternalServerError);

                response.HttpResponseHeaders.Status = HttpStatus.InternalServerError;

                request.Params.Add(nameof(Exception), ex);

                var temp = await Middleware.ActionExecuting(context, request, executedCommand);

                await response.WriteBodyAsync(temp);
            }
            finally
            {
                await NetworkStream.FlushAsync();
            }
        }
    }
}