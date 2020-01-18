using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.BaseContex;
using Microsoft.Extensions.Logging;
using System.IO;

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
                await RequestProcessing();
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
            TcpClient client = await TcpListener.AcceptTcpClientAsync();

            logger.LogInformation($"New connection from: {client.Client.RemoteEndPoint}");

            // Get a stream object for reading and writing
            NetworkStream netStream = client.GetStream();
            netStream.ReadTimeout = 500;

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

            using (Context context = await ContextBuilder.CreateContext(executedCommand.ClassExecuted, netStream, request, ServiceProvider))
            {
                // context.Response.EnableCrossOriginRequests();
                try
                {
                    await InvokeHandlerAsync(context, executedCommand);
                    await context.WriteHeadersAsync();
                    await context.NetworkStream.FlushAsync();
                }
                catch (IOException _)
                {
                    // May happen but it's not really important
                }
                catch(Exception ex)
                {
                    logger.LogError(exception: ex, message: "Internal Server Error");
                    await InvokeHandlerAsync(context, InternalServerErrorHandler, ex);
                    await context.WriteHeadersAsync();
                    await context.NetworkStream.FlushAsync();
                }
            }
            client.Close();
        }

        private async Task InvokeHandlerAsync(Context context, ExecutedCommand executedCommand, params object[] args)
        {
            if (executedCommand.MethodExecuted.Invoke(context, args) is object data)
            {
                if (data is Task<dynamic> task)
                {
                    data = await task;
                }

                if (data is JsonResult jsonSerializable)
                {
                    await context.SendJson(jsonSerializable);
                }
                else if (data is object)
                {
                    await context.SendText(data);
                }

                if (context.Response.Status == HttpStatus.Ok)
                {
                    context.Response.Status = HttpStatus.NoContent;
                }
            }
        }

        private ExecutedCommand RequestedEndpoint(Request request)
        {
            for (int index = 0; index < Endpoints.Length; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.Method == request.Method && request.Path.Equals(currentEndpoint.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return currentEndpoint;
                }
            }
            return null;
        }
    }
}