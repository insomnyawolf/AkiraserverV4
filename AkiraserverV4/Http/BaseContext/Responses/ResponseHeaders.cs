using Extensions;
using AkiraserverV4.Http.Helper;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace AkiraserverV4.Http.Context.Responses
{
    public partial class Response
    {
        public HttpVersion ProtocolVersion { get; set; } = HttpVersion.HTTP11;
        public HttpStatus Status { get; set; } = HttpStatus.Ok;
        public Dictionary<string, string> Headers { get; set; }
        private const string HeaderSeparator = "\r\n";

        public Response(ResponseSettings settings, NetworkStream NetworkStream)
        {
            this.NetworkStream = NetworkStream;
            this.StreamWriter = new StreamWriter(NetworkStream, Encoding.UTF8);
            Headers = settings.StaticResponseHeaders?.DeepClone() ?? new Dictionary<string, string>();
        }

        public string ProcessHeaders()
        {
            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.Append(ProtocolVersion.ToVersionString());
            headerBuilder.Append(' ');
            headerBuilder.Append(Status.ToStatusString());
            headerBuilder.Append(HeaderSeparator);

            foreach (KeyValuePair<string, string> header in Headers)
            {
                string key = header.Key;
                string value = header.Value;

                if (key == HeaderNames.ContentLength && Status == HttpStatus.NoContent)
                {
                    value = "0";
                }

                headerBuilder.Append(key);
                headerBuilder.Append(": ");
                headerBuilder.Append(value);
                headerBuilder.Append(HeaderSeparator);
            }

            headerBuilder.Append(HeaderSeparator);

            return headerBuilder.ToString();
        }

        public void AddContentType(ContentType contentType)
        {
            if (!Headers.ContainsKey(HeaderNames.ContentType))
            {
                Headers.Add(HeaderNames.ContentType, Mime.ToString(contentType));
            }
        }

        public void AddContentLenght(int lenght)
        {
            if (!Headers.ContainsKey(HeaderNames.ContentLength))
            {
                Headers.Add(HeaderNames.ContentLength, lenght.ToString());
            }
        }

        public void AddContentLenght(long lenght)
        {
            if (!Headers.ContainsKey(HeaderNames.ContentLength))
            {
                Headers.Add(HeaderNames.ContentLength, lenght.ToString());
            }
        }

        public void AddContentDisposition(string value)
        {
            if (!Headers.ContainsKey(HeaderNames.ContentDisposition))
            {
                Headers.Add(HeaderNames.ContentDisposition, value);
            }
        }
    }
}