using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public class TcpListenerEnumerable : TcpListener, IAsyncEnumerable<TcpClient>
    {
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        public async IAsyncEnumerator<TcpClient> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested && !CancellationTokenSource.IsCancellationRequested)
            {
                yield return await AcceptTcpClientAsync();
            }
        }

        public TcpListenerEnumerable(IPAddress localaddr, int port) : base (localaddr, port)
        {

        }
    }

    public partial class AkiraServerV4
    {
        private readonly ILogger<AkiraServerV4> Logger;

        public bool IsListening { get; private set; }

        private readonly TcpListenerEnumerable TcpListener;
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

            TcpListener = new TcpListenerEnumerable(localaddr: IPAddress.Any, port: GeneralSettings.Port);

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

            if (TcpListener.CancellationTokenSource.IsCancellationRequested)
            {
                TcpListener.CancellationTokenSource.TryReset();
            }

            TcpListener.Start();
            IsListening = true;

            Logger.LogInformation($"Now Listening on '{TcpListener.LocalEndpoint}'...");

//            await Parallel.ForEachAsync(TcpListener, async (TcpClient, CancelationToken) =>
//            {
//                try
//                {
//                    await ServeNext(TcpClient);
//                }
//                catch (Exception e) when (e is SocketException || e is IOException)
//                {
//#if DEBUG
//                    Logger.LogError(e.ToString());
//#endif
//                }
//            });

//            while (IsListening)
//            {
//                try
//                {
//                    var TcpClient = await TcpListener.AcceptTcpClientAsync(TcpListener.CancellationTokenSource.Token);
//                    ThreadPool.UnsafeQueueUserWorkItem(async (client) =>
//                    {
//                        await ServeNext(client);
//                    }, TcpClient, preferLocal: false);
//                }
//                catch (Exception e) when (e is SocketException || e is IOException)
//                {
//#if DEBUG
//                    Logger.LogError(e.ToString());
//#endif
//                }
//            }

            while (IsListening)
            {
                try
                {
                    var TcpClient = await TcpListener.AcceptTcpClientAsync(TcpListener.CancellationTokenSource.Token);


                    _ = Task.Run(async() =>
                    {
                        await ServeNext(TcpClient);
                    });
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
            TcpListener.CancellationTokenSource.Cancel();
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

        public async Task ServeNext(TcpClient TcpClient)
        {
            try
            {
                Logger.LogInformation($"New connection from: {TcpClient.Client.RemoteEndPoint}");
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

                    //netStream.ReadTimeout = GeneralSettings.RequestSettings.ReciveTimeout;
                    //netStream.WriteTimeout = GeneralSettings.ResponseSettings.SendTimeout;

                    // Stream Checks =================================================================

                    //using var bufferedStream = new BufferedStream(netStream, GeneralSettings.BufferSize);

                    await ProcessRequest(netStream);
                }
            }
            catch (IOException)
            {
                // dropped connections and such i guess
            }

        }

        public async Task ProcessRequest(NetworkStream NetworkStream)
        {
            var request = Request.TryParseRequest(NetworkStream, GeneralSettings.RequestSettings);

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
                throw;
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