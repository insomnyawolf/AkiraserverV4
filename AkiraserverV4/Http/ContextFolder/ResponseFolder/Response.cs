using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.ContextFolder.ResponseFolder
{
    public class Response
    {
        public HttpVersion ProtocolVersion { get; set; }
        public HttpStatus Status { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        private const string HeaderSeparator = "\r\n";

        public Response(HttpStatus status = HttpStatus.Ok, HttpVersion protocolVersion = HttpVersion.HTTP11)
        {
            Headers = new Dictionary<string, string>();
            Status = status;
            ProtocolVersion = protocolVersion;
        }

        public string ProcessHeaders()
        {
            if (!Headers.ContainsKey("Content-Length"))
            {
                throw new Exception("You must add the content lenght header first");
            }

            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.Append(ProtocolVersion.ToVersionrString());
            headerBuilder.Append(Status.ToStatusString());
            headerBuilder.Append(HeaderSeparator);

            foreach (KeyValuePair<string, string> header in Headers)
            {
                headerBuilder.Append(header.Key);
                headerBuilder.Append(": ");
                headerBuilder.Append(header.Value);
                headerBuilder.Append(HeaderSeparator);
            }

            headerBuilder.Append(HeaderSeparator);

            return headerBuilder.ToString();
        }

        public void AddContentLenghtHeader(int lenght)
        {
            Headers.Add("Content-Length", lenght.ToString());
        }
    }
}