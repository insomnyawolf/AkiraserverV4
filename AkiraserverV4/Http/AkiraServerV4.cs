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
using System.Threading;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
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

            if (CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.TryReset();
            }

            TcpListener.Start();
            IsListening = true;

            Logger.LogInformation($"Now Listening on '{TcpListener.LocalEndpoint}'...");

            //while (!CancellationTokenSource.IsCancellationRequested)
            //{
            //    var TcpClient = await TcpListener.AcceptTcpClientAsync(CancellationTokenSource.Token);

            //    ThreadPool.QueueUserWorkItem(async (client) =>
            //    {
            //        await ServeNext(client);
            //    }, TcpClient, preferLocal: false);
            //}

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    TcpClient TcpClient = await TcpListener.AcceptTcpClientAsync(CancellationTokenSource.Token);

                    _ = Task.Run(() => ServeNext(TcpClient), CancellationTokenSource.Token);
                }
                catch
                {
                    // Discard exceptions caused by clients disconnecting abruptly
                }
                
            }

            TcpListener.Stop();
        }

        public void StopListening()
        {
            IsListening = false;
            CancellationTokenSource.Cancel();
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

        public async ValueTask ServeNext(TcpClient TcpClient)
        {
            try
            {
#if DEBUG
                Logger.LogInformation($"New connection from: {TcpClient.Client.RemoteEndPoint}");
#endif
                // Get a stream object for reading and writing
                using (NetworkStream netStream = TcpClient.GetStream())
                {

                    // Stream Checks =================================================================
                    if (!netStream.CanRead)
                    {
                        Logger.LogCritical("Can Not Read Stream".ToErrorString(this));
                        netStream.Close();
                        TcpClient.Close();
                        return;
                    }

                    if (!netStream.CanWrite)
                    {
                        Logger.LogCritical("Can Not Write To The Stream".ToErrorString(this));
                        netStream.Close();
                        TcpClient.Close();
                        return;
                    }

                    netStream.ReadTimeout = GeneralSettings.RequestSettings.ReciveTimeout;
                    netStream.WriteTimeout = GeneralSettings.ResponseSettings.SendTimeout;

                    // Stream Checks =================================================================

                    await ProcessRequest(netStream);
                }
            }
            catch
            {
                // Discard exceptions caused by clients disconnecting abruptly
            }
            finally
            {
                TcpClient.Close();
                TcpClient.Dispose();
            }
        }

        public async ValueTask ProcessRequest(NetworkStream NetworkStream)
        {
            var request = await Request.TryParseRequest(NetworkStream, GeneralSettings.RequestSettings);
#if DEBUG
            //request.LogPacket(ServiceProvider.GetRequiredService<ILogger<Request>>());
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
                var temp = await Middleware.Next(context, request, executedCommand);

                if (response.HttpResponseHeaders.Status == HttpStatus.Unset)
                {
                    response.HttpResponseHeaders.Status = HttpStatus.Ok;
                }

                await response.WriteBodyAsync(temp);
            }
            catch (IOException)
            {
                throw;
                // dropped connections and such i guess
            }
            catch (Exception ex)
            {
                executedCommand = GetEndpoint(SpecialEndpoint.InternalServerError);

                response.HttpResponseHeaders.Status = HttpStatus.InternalServerError;

                request.Params.Add(nameof(Exception), ex);

                var temp = await Middleware.Next(context, request, executedCommand);

                await response.WriteBodyAsync(temp);
            }
            finally
            {
                await NetworkStream.FlushAsync();
            }
        }
    }
}