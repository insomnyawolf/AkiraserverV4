using Microsoft.Extensions.Configuration;
using SuperSimpleHttpListener.Http.Extensions;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSimpleHttpListener.Http
{
    public class Listener
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
                await RequestProcessing();
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

            List<byte> buffer = new List<byte>();

            do
            {
                const int defaultSize = 8192;
                byte[] currentBuffer = new byte[defaultSize];
                int dataRead = await netStream.ReadAsync(buffer: currentBuffer);

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
            } while (netStream.DataAvailable);

#if DEBUG
            var data = System.Text.Encoding.ASCII.GetString(buffer.ToArray(), 0, buffer.Count);
            Console.WriteLine("Received: {0}", data);
#endif

            Request.Request Request = new Request.Request(buffer.ToArray());

            if (!netStream.CanWrite)
            {
                Console.WriteLine("Can Not Write To The Stream".ToErrorString(this));
                netStream.Close();
                client.Close();
                return;
            }

            var response = new Response.Response();
            response.Body = "Hello World".ToByteArray();

            byte[] rawResponse = response.ToBytes();

            await netStream.WriteAsync(rawResponse, 0, rawResponse.Length);
            await netStream.FlushAsync();

            netStream.Close();
            client.Close();
        }

        public void StopListening()
        {
            IsListening = false;
        }
    }

    internal static class HelperTest
    {
    }
}