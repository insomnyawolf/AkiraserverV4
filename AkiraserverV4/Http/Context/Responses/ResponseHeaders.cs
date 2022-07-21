using Extensions;
using AkiraserverV4.Http.Helper;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace AkiraserverV4.Http.Context.Responses
{
    public partial class Response
    {
        public HttpResponseHeaders HttpResponseHeaders { get; set; }
        public NetworkStream NetworkStream { get; private set; }
        public StreamWriter StreamWriter { get; private set; }

        private bool HeadersWritten;

        public Response(ResponseSettings settings, NetworkStream NetworkStream)
        {
            this.NetworkStream = NetworkStream;
            this.StreamWriter = new StreamWriter(NetworkStream, Encoding.UTF8);
            HttpResponseHeaders = settings.StaticHttpResponseHeaders?.DeepClone() ?? new HttpResponseHeaders();
        }

        public void AddContentType(ContentType contentType)
        {
            if (!HttpResponseHeaders.ContainsKey(HeaderNames.ContentType))
            {
                HttpResponseHeaders.Add(HeaderNames.ContentType, Mime.ToString(contentType));
            }
        }

        public void AddContentLenght(int lenght)
        {
            if (!HttpResponseHeaders.ContainsKey(HeaderNames.ContentLength))
            {
                HttpResponseHeaders.Add(HeaderNames.ContentLength, lenght.ToString());
            }
        }

        public void AddContentLenght(long lenght)
        {
            if (!HttpResponseHeaders.ContainsKey(HeaderNames.ContentLength))
            {
                HttpResponseHeaders.Add(HeaderNames.ContentLength, lenght.ToString());
            }
        }

        public void AddContentDisposition(string value)
        {
            if (!HttpResponseHeaders.ContainsKey(HeaderNames.ContentDisposition))
            {
                HttpResponseHeaders.Add(HeaderNames.ContentDisposition, value);
            }
        }
    }
}