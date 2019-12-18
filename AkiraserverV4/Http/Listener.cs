﻿using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.ContextFolder.RequestFolder;
using AkiraserverV4.Http.ContextFolder.ResponseFolder;
using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
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
        private Endpoint[] Endpoints;

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
            ValidateRouting();
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

        private void LoadRouting()
        {
            List<Endpoint> endpoints = new List<Endpoint>();

            Type[] classes = Assembly.GetEntryAssembly().GetTypes();
            for (int classIndex = 0; classIndex < classes.Length; classIndex++)
            {
                Type currentClass = classes[classIndex];
                ControllerAttribute controllerAttribute = currentClass.GetCustomAttribute<ControllerAttribute>();
                if (controllerAttribute != null)
                {
                    MethodInfo[] methods = currentClass.GetMethods();
                    for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
                    {
                        MethodInfo currentMethod = methods[methodIndex];

                        DefaultEndpointAttribute defaultEndpointAttribute = currentMethod.GetCustomAttribute<DefaultEndpointAttribute>();
                        BaseEndpointAttribute endpointAttribute = currentMethod.GetCustomAttribute<BaseEndpointAttribute>();
                        if (endpointAttribute != null)
                        {
                            string controllerPath = controllerAttribute.Path.Replace("[controller]", currentClass.Name);
                            string methodPath = endpointAttribute.Path.Replace("[method]", currentMethod.Name);
                            string path = controllerPath + methodPath;
                            endpoints.Add(new Endpoint()
                            {
                                ClassExecuted = currentClass,
                                MethodExecuted = currentMethod,
                                Method = endpointAttribute.Method,
                                Path = path,
                                Priority = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length
                            });
                        }
                        else if (defaultEndpointAttribute != null)
                        {
                            if (DefaultCommand != null)
                            {
                                throw new MultipleDefaultEndpointException();
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

            // Ordena los endpoints de mas especificos a menos especificos
            endpoints.Sort((x, y) =>
            {
                if (x.Priority > y.Priority)
                {
                    return -1; //normally greater than = 1
                }

                if (x.Priority < y.Priority)
                {
                    return 1; // normally smaller than = -1
                }
                return 0; // equal
            });

            Endpoints = endpoints.ToArray();
        }

        private void ValidateRouting()
        {
            List<EndpointCount> duplicatedCheck = new List<EndpointCount>();

            for (int index = 0; index < Endpoints.Length; index++)
            {
                Endpoint endpoint = Endpoints[index];

                bool exists = false;

                for (int duplicatedCheckIndex = 0; duplicatedCheckIndex < duplicatedCheck.Count; duplicatedCheckIndex++)
                {
                    if (endpoint.Path.Equals(duplicatedCheck[duplicatedCheckIndex].Path, StringComparison.InvariantCultureIgnoreCase)
                        && endpoint.Method == duplicatedCheck[duplicatedCheckIndex].Method)
                    {
                        exists = true;
                        duplicatedCheck[duplicatedCheckIndex].Count++;
                    }
                }

                if (!exists)
                {
                    duplicatedCheck.Add(new EndpointCount()
                    {
                        Path = endpoint.Path,
                        Method = endpoint.Method
                    });
                }
            }

            StringBuilder error = new StringBuilder();
            for (int i = 0; i < duplicatedCheck.Count; i++)
            {
                EndpointCount item = duplicatedCheck[i];
                if (item.Count > 0)
                {
                    // $"* Route: '{item.Method} => {item.Path} ' appears '{item.Count + 1}' times ."
                    error.Append("* Route: '").Append(item.Method).Append(" => ").Append(item.Path).Append(" ' appears '").Append(item.Count + 1).Append("' times .");
                }
            }

            string errorString = error.ToString();

            if (!string.IsNullOrEmpty(errorString))
            {
                throw new RoutingException(errorString);
            }

            if (DefaultCommand is null)
            {
                throw new NoDefaultEndpointException();
            }

#if DEBUG
            StringBuilder sb = new StringBuilder();
            sb.Append("Loaded The following Endpoints:\n");
            foreach (Endpoint endpoint in Endpoints)
            {
                sb.Append("* Route: '").Append(endpoint.Method).Append(" => ").Append(endpoint.Path).Append("'.\n");
            }
            Console.WriteLine(sb.ToString());
#endif
        }

        private class EndpointCount
        {
            public string Path { get; set; }
            public HttpMethod Method { get; set; }
            public int Count { get; set; }
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
                context.Response.EnableCrossOriginRequests();

                if (executedCommand.MethodExecuted.Invoke(context, null) is object data)
                {
                    if (data is Task task)
                    {
                        if (await (dynamic)data is object awaitedData)
                        {
                            await context.SendObject(awaitedData);
                        }
                    }
                    else
                    {
                        await context.SendObject(data);
                    }
                }
                else
                {
                    context.Response.Status = HttpStatus.NoContent;
                    await context.WriteHeaders();
                }
            }

            client.Close();
        }

        private ExecutedCommand RequestedEndpoint(Request request)
        {
            for (int index = 0; index < Endpoints.Length; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.Method == request.Method
                 && request.Path.StartsWith(currentEndpoint.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return currentEndpoint;
                }
            }
            return DefaultCommand;
        }
    }
}