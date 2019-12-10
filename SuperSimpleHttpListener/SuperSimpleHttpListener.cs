using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SuperSimpleHttpListener
{
    public class SuperSimpleHttpListener
    {
        public bool IsListening { get; private set; }
        public HttpListener Listener { get; set; }

        public SuperSimpleHttpListener(IConfigurationSection configuration)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
            }

            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Create a listener.
            Listener = new HttpListener();

            // Add the prefixes.
            foreach (KeyValuePair<string, string> s in configuration.GetSection("Adresses").AsEnumerable())
            {
                if (!string.IsNullOrWhiteSpace(s.Value))
                {
                    Listener.Prefixes.Add(s.Value);
                }
            }

            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (Listener.Prefixes.Count == 0)
            {
                throw new ArgumentException($"No valid prefixes in {nameof(configuration)}");
            }
        }

        public void StartListening()
        {
            Listener.Start();
            IsListening = true;

            while (IsListening)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = Listener.GetContext();
                HttpListenerRequest request = context.Request;
                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }

            Listener.Stop();
        }

        public void StopListening()
        {
            IsListening = false;
        }
    }
}