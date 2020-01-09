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
using static AkiraserverV4.Http.BaseContex.BaseContext;

namespace AkiraserverV4.Http
{
    public partial class Listener
    {
        public bool IsListening { get; private set; }

        private readonly TcpListener TcpListener;
        private readonly ServiceProvider ServiceProvider;

        public Listener(ServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            IConfigurationSection configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server");

            string portStr = configuration.GetSection("Port").Value;
            if (!int.TryParse(portStr, out int port))
            {
                throw new ArgumentException($"Invalid {nameof(port)} -> '{portStr}'");
            }

            LoadDefaultRouting();

            // Create a listener.
            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: port);
        }

        public Listener(ServiceProvider serviceProvider, Assembly assembly) : this(serviceProvider)
        {
            LoadRouting(assembly);
        }

        public async Task StartListening()
        {
            if (Endpoints is null)
            {
                Console.WriteLine("There are no endpoints loaded".ToErrorString(this));
            }

            if (IsListening)
            {
                Console.WriteLine("Already Listening".ToErrorString(this));
                return;
            }

            TcpListener.Start();
            IsListening = true;

            Console.WriteLine($"Now Listening on '{TcpListener.LocalEndpoint}'...\n");

            while (IsListening)
            {
#if RELEASE
                try
                {
#endif
                await RequestProcessing();
#if RELEASE
                }
                catch (Exception e)
                {
                    Console.WriteLine(FormatException(e));
                    static string FormatException(Exception exception)
                    {
                        if (exception.InnerException != null)
                        {
                            return $"{exception.Message} -> {FormatException(exception.InnerException)}";
                        }
                        return $"{exception.Message}";
                    }
                }
#endif
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

#if DEBUG
            Console.WriteLine($"New connection from: {client.Client.RemoteEndPoint}");
#endif

            // Get a stream object for reading and writing
            NetworkStream netStream = client.GetStream();

            if (!netStream.CanRead)
            {
                Console.WriteLine("Can Not Read Stream".ToErrorString(this));
                netStream.Close();
                client.Close();
                return;
            }

            if (!netStream.CanWrite)
            {
                Console.WriteLine("Can Not Write To The Stream".ToErrorString(this));
                netStream.Close();
                client.Close();
                return;
            }

            Request request = new Request(netStream);

            ExecutedCommand executedCommand = RequestedEndpoint(request);
            if (executedCommand is null)
            {
                executedCommand = NotFound;
            }

            using (BaseContext context = await ContextBuilder.CreateContext(executedCommand.ClassExecuted, netStream, request, ServiceProvider))
            {
                // context.Response.EnableCrossOriginRequests();
                if (executedCommand.MethodExecuted.Invoke(context, null) is object data)
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

                await context.WriteHeadersAsync();

                await context.NetworkStream.FlushAsync();
            }
            client.Close();
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