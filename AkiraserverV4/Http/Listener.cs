using AkiraserverV4.Http.Anotations;
using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
#warning Temporal Placeholder
    public class Listener<T> where T : Context, new()
    {
        public bool IsListening { get; private set; }
        public TcpListener TcpListener { get; set; }

        public Listener(IConfigurationSection configuration)
        {
            // URI prefixes are required,
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string portStr = configuration.GetSection("Port").Value;
            if (!int.TryParse(portStr, out int port))
            {
                throw new ArgumentException($"Invalid {nameof(port)} -> '{portStr}'");
            }
            // Create a listener.
            TcpListener = new TcpListener(localaddr: IPAddress.Any, port: port);
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

        public async Task RequestProcessing()
        {
            //Listener.AcceptSocketAsync
            TcpClient client = await TcpListener.AcceptTcpClientAsync();
            client.NoDelay = true;
            client.ReceiveTimeout = 250;
            client.SendTimeout = 250;

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

            using (T context = await ContextBuilder.CreateContext<T>(netStream))
            {
#warning Make Routing
#warning This is a Temporal Placeholder
                var methods = typeof(T).GetMethods();

                for (int i = 0; i < methods.Length; i++)
                {
                    var attrib = methods[i].GetCustomAttribute(typeof(DefaultRoutingAttribute));

                    if (attrib != null)
                    {
                        methods[i].Invoke(context, null);
                    }
                }
            }

            client.Close();
        }

        public void StopListening()
        {
            IsListening = false;
        }
    }
}