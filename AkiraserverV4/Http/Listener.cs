using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.ContextFolder.RequestFolder;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using static AkiraserverV4.Http.ContextFolder.Context;

namespace AkiraserverV4.Http
{
    public class Listener
    {
        public bool IsListening { get; private set; }

        private readonly TcpListener TcpListener;
        private readonly ServiceProvider ServiceProvider;

        private ExecutedCommand DefaultCommand;
        private List<Endpoint> Endpoints;

        public Listener(ServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            IConfigurationSection configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Server");

            string portStr = configuration.GetSection("Port").Value;
            if (!int.TryParse(portStr, out int port))
            {
                throw new ArgumentException($"Invalid {nameof(port)} -> '{portStr}'");
            }
            // Create a listener.
            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: port);

            LoadRouting();
        }

        public async Task StartListening()
        {
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
                try
                {
                    await RequestProcessing();
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
            }

            TcpListener.Stop();
        }

        private void LoadRouting()
        {
            Endpoints = new List<Endpoint>();

            Type[] classes = Assembly.GetEntryAssembly().GetTypes();
            for (int classIndex = 0; classIndex < classes.Length; classIndex++)
            {
                Type currentClass = classes[classIndex];
                ControllerAttribute controllerAttribute = currentClass.GetCustomAttribute<ControllerAttribute>();
                if (controllerAttribute != null)
                {
                    MethodInfo[] methods = currentClass.GetMethods();
                    for (int methodIndex = 0; methodIndex < classes.Length; methodIndex++)
                    {
                        MethodInfo currentMethod = methods[methodIndex];

                        DefaultEndpointAttribute defaultEndpointAttribute = currentMethod.GetCustomAttribute<DefaultEndpointAttribute>();
                        BaseEndpointAttribute endpointAttribute = currentMethod.GetCustomAttribute<BaseEndpointAttribute>();
                        if (endpointAttribute != null)
                        {
                            Endpoints.Add(new Endpoint()
                            {
                                ClassExecuted = currentClass,
                                MethodExecuted = currentMethod,
                                Method = endpointAttribute.Method,
                                Path = controllerAttribute.Path + endpointAttribute.Path,
                            });
                        }
                        else if (defaultEndpointAttribute != null)
                        {
                            if (DefaultCommand != null)
                            {
                                throw new Exception("Multiple Default Fallbacks Found");
                            }

                            DefaultCommand = new ExecutedCommand()
                            {
                                ClassExecuted = currentClass,
                                MethodExecuted = currentMethod,
                            };
                        }
                    }
                }
            }

            if (DefaultCommand is null)
            {
                throw new Exception("Fallback not found");
            }
        }

        public async Task RequestProcessing()
        {
            //Listener.AcceptSocketAsync
            TcpClient client = await TcpListener.AcceptTcpClientAsync();
            client.NoDelay = true;
            client.ReceiveTimeout = 250;
            client.SendTimeout = 250;

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

            Request request = await ParseRequest(netStream);

            ExecutedCommand executedCommand = RequestedEndpoint(request);

            using (Context context = await ContextBuilder.CreateContext(executedCommand.ClassExecuted, netStream, request, ServiceProvider))
            {
                executedCommand.MethodExecuted.Invoke(context, null);
            }

            client.Close();
        }

        public void StopListening()
        {
            IsListening = false;
        }

        private async Task<Request> ParseRequest(NetworkStream networkStream)
        {
            List<byte> buffer = new List<byte>();

            do
            {
                const int defaultSize = 8192;
                byte[] currentBuffer = new byte[defaultSize];
                int dataRead = await networkStream.ReadAsync(buffer: currentBuffer);

                if (dataRead == defaultSize)
                {
                    buffer.AddRange(currentBuffer);
                }
                else
                {
                    byte[] partialBuffer = new byte[dataRead];
                    Buffer.BlockCopy(currentBuffer, 0, partialBuffer, 0, dataRead);
                    buffer.AddRange(partialBuffer);
                }
            } while (networkStream.DataAvailable);

            return new Request(buffer.ToArray());
        }

        private ExecutedCommand RequestedEndpoint(Request request)
        {
            for (int index = 0; index < Endpoints.Count; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.Method == request.Method
                    && currentEndpoint.Path == request.Path)
                {
                    return currentEndpoint;
                }
            }
            return DefaultCommand;
        }
    }
}