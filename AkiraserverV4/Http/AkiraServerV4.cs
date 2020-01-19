using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.BaseContex.Requests;
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
        private readonly ILogger<AkiraServerV4> logger;

        public bool IsListening { get; private set; }

        private readonly TcpListener TcpListener;
        private readonly ServiceProvider ServiceProvider;

        public AkiraServerV4(ServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var LoggerFactory = ServiceProvider.GetService<ILoggerFactory>() ?? throw new NullReferenceException("Logger service was not found on the dependence injection.");

            logger = LoggerFactory.CreateLogger<AkiraServerV4>();

            IConfigurationSection configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server");

            string portStr = configuration.GetSection("Port").Value;
            if (!int.TryParse(portStr, out int port))
            {
                throw new ArgumentException($"Invalid {nameof(port)} -> '{portStr}'");
            }

            LoadDefaultRouting();

            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: port);
        }

        public AkiraServerV4(ServiceProvider serviceProvider, Assembly assembly) : this(serviceProvider)
        {
            LoadRouting(assembly);
        }

        public async Task StartListening()
        {
            if (Endpoints is null)
            {
                logger.LogError("There are no endpoints loaded".ToErrorString(this));
                return;
            }

            if (IsListening)
            {
                logger.LogWarning("Already Listening".ToErrorString(this));
                return;
            }

            TcpListener.Start();
            IsListening = true;

            logger.LogInformation($"Now Listening on '{TcpListener.LocalEndpoint}'...");

            while (IsListening)
            {
                try
                {
                    await RequestProcessing().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Something Failed On The Listener");
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
                logger.LogInformation($"New connection from: {client.Client.RemoteEndPoint}");

                // Get a stream object for reading and writing
                NetworkStream netStream = client.GetStream();

#warning Convert to configurable value
                netStream.ReadTimeout = 500;

                // Stream Checks =================================================================
                if (!netStream.CanRead)
                {
                    logger.LogCritical("Can Not Read Stream".ToErrorString(this));
                    netStream.Close();
                    client.Close();
                    return;
                }

                if (!netStream.CanWrite)
                {
                    logger.LogCritical("Can Not Write To The Stream".ToErrorString(this));
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
                    // context.Response.EnableCrossOriginRequests();

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
                        logger.LogError(exception: exception, message: "Internal Server Error");
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