using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AkiraserverV4.Http.ContextFolder.RequestFolder
{
    public class Request
    {
        private static readonly byte[] HeaderSeparator = Encoding.UTF8.GetBytes("\r\n\r\n");

        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public byte[] Body { get; set; }

        public Request(byte[] raw)
        {
            var requestParts = raw.Separate(HeaderSeparator, 1);

            if (requestParts.Length > 0)
            {
                Headers = new Dictionary<string, string>();
                ParseHeaders(requestParts[0]);
            }

            if (requestParts.Length > 1)
            {
                Body = requestParts[1];
            }
        }

        private void ParseHeaders(byte[] rawheaders)
        {
            StringReader sr = new StringReader(Encoding.UTF8.GetString(rawheaders));
            string[] firstLine = sr.ReadLine().Split(' ');
            Method = HttpMethodConvert.FromString(firstLine[0]);
            Path = firstLine[1];
            Version = HttpVersionConvert.FromString(firstLine[2]);

            string currentline;
            while ((currentline = sr.ReadLine()) != null)
            {
                string[] header = currentline.Split(": ");
                if (header.Length != 2)
                {
                    throw new ArgumentException($"The header '{currentline}' is not valid.");
                }
                Headers.Add(header[0], header[1]);
            }
        }
    }
}