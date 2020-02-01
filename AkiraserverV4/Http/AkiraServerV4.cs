using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
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
        private readonly IConfigurationSection Configuration;

        public AkiraServerV4(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger<AkiraServerV4>();

            Configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server");

            string Port = Configuration.GetSection(nameof(Port)).Value;
            if (!int.TryParse(Port, out int _Port))
            {
                throw new ArgumentException($"Invalid {nameof(Port)} -> '{Port}'");
            }

            LoadDefaultRouting();

            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: _Port);

            ReloadServerConfig();
        }

        public void ReloadServerConfig()
        {
            string ExclusiveAddressUse = Configuration.GetSection(nameof(ExclusiveAddressUse)).Value;
            if (bool.TryParse(ExclusiveAddressUse, out bool _ExclusiveAddressUse))
            {
                TcpListener.Server.ExclusiveAddressUse = _ExclusiveAddressUse;
            }
            else if (!string.IsNullOrEmpty(ExclusiveAddressUse))
            {
                throw new ArgumentException($"Invalid {nameof(ExclusiveAddressUse)} -> '{ExclusiveAddressUse}'");
            }

            string ReciveTimeout = Configuration.GetSection(nameof(ReciveTimeout)).Value;
            if (int.TryParse(ReciveTimeout, out int _ReciveTimeout))
            {
                TcpListener.Server.ReceiveTimeout = _ReciveTimeout;
            }
            else if (!string.IsNullOrEmpty(ReciveTimeout))
            {
                throw new ArgumentException($"Invalid {nameof(ReciveTimeout)} -> '{ReciveTimeout}'");
            }

            string SendTimeout = Configuration.GetSection(nameof(SendTimeout)).Value;
            if (int.TryParse(SendTimeout, out int _SendTimeout))
            {
                TcpListener.Server.ReceiveTimeout = _SendTimeout;
            }
            else if (!string.IsNullOrEmpty(SendTimeout))
            {
                throw new ArgumentException($"Invalid {nameof(SendTimeout)} -> '{SendTimeout}'");
            }

            string Ttl = Configuration.GetSection(nameof(Ttl)).Value;
            if (short.TryParse(Ttl, out short _Ttl))
            {
                TcpListener.Server.Ttl = _Ttl;
            }
            else if (!string.IsNullOrEmpty(Ttl))
            {
                throw new ArgumentException($"Invalid {nameof(Ttl)} -> '{Ttl}'");
            }

            TcpListener.Server.UseOnlyOverlappedIO = true;
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
                catch (Exception e)
                {
                    Logger.LogCritical(e, "Something Failed On The Listener");
                }
            }

            TcpListener.Stop();
        }

        public void StopListening()
        {
            IsListening = false;
        }

        public async Task RequestProcessing()
        {
            //Listener.AcceptSocketAsync
            using (TcpClient client = await TcpListener.AcceptTcpClientAsync().ConfigureAwait(false))
            {
                Logger.LogInformation($"New connection from: {client.Client.RemoteEndPoint}");
                // Get a stream object for reading and writing
                NetworkStream netStream = client.GetStream();

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

                ExecutedCommand executedCommand;
                Request request = await Request.ParseRequest(netStream);
                if (request is null)
                {
                    executedCommand = BadRequestHandler;
                }
                else if (RequestedEndpoint(request) is ExecutedCommand executedCommand1)
                {
                    executedCommand = executedCommand1;
                }
                else
                {
                    executedCommand = NotFoundHandler;
                }

                using (Context context = ContextBuilder.CreateContext(executedCommand.ClassExecuted, netStream, request, ServiceProvider))
                {
                    context.Response.EnableCrossOriginRequests();

                    bool connectionAborted = false;

                    try
                    {
                        await InvokeHandlerAsync(context, executedCommand).ConfigureAwait(false);
                    }
                    catch (IOException)
                    {
                        // Not really important
                        // It happens when the client force closes the connection
                        connectionAborted = true;
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError(exception: exception, message: "Internal Server Error");
                        await InvokeHandlerAsync(context, InternalServerErrorHandler, exception).ConfigureAwait(false);
                    }

                    if (!connectionAborted)
                    {
                        await context.WriteHeadersAsync().ConfigureAwait(false);
                        await context.NetworkStream.FlushAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}