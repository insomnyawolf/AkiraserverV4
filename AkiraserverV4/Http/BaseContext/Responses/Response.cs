﻿using SuperSimpleHttpListener.Http.Helper;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.BaseContext.Responses
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
            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.Append(ProtocolVersion.ToVersionString());
            headerBuilder.Append(' ');
            headerBuilder.Append(Status.ToStatusString());
            headerBuilder.Append(HeaderSeparator);

            foreach (KeyValuePair<string, string> header in Headers)
            {
                string key = header.Key;
                string value = header.Value;

                if (key == "Content-Length" && Status == HttpStatus.NoContent)
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

        public void AddContentLenghtHeader(int lenght)
        {
            Headers.Add("Content-Length", lenght.ToString());
        }

        public void EnableCrossOriginRequests(string host = "*")
        {
            Headers.Add("Access-Control-Allow-Origin", host);
        }
    }
}