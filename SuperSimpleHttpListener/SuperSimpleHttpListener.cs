using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperSimpleHttpListener
{
    public class SuperSimpleHttpListener
    {
        public bool IsListening { get; private set; }
        public TcpListener Listener { get; set; }

        public SuperSimpleHttpListener(IConfigurationSection configuration)
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
            Listener = new TcpListener(localaddr: IPAddress.Any, port: port);
        }

        public void StartListening()
        {
            Listener.Start();
            IsListening = true;

            while (IsListening)
            {
                RequestProcessing();
            }

            Listener.Stop();
        }

        public async Task RequestProcessing()
        {
            TcpClient client = await Listener.AcceptTcpClientAsync();

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            Memory<byte> buffer = new Memory<byte>();
            int lenght = await stream.ReadAsync(buffer: buffer);

            var data = System.Text.Encoding.ASCII.GetString(buffer.ToArray(), 0, lenght);
            Console.WriteLine("Received: {0}", data);

            // Process the data sent by the client.
            data = data.ToUpper();

            byte[] response = data.ToByteArray();
            await stream.WriteAsync(response, 0, response.Length);

            client.Close();
        }

        public void StopListening()
        {
            IsListening = false;
        }
    }

    internal static class HelperTest
    {
        public static byte[] ToByteArray(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
    }
}