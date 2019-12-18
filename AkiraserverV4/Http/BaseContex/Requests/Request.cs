using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

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
            byte[][] requestParts = raw.Separate(HeaderSeparator, 1);

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

        public Dictionary<string, string> GetUrlEncodedForm()
        {
            string rawBody = Encoding.UTF8.GetString(Body);
            return DeserializeUrlEncoded(rawBody);
        }

        public Dictionary<string, string> GetQueryUrl()
        {
            string[] query = Path.Split('?', StringSplitOptions.RemoveEmptyEntries);

            if (query.Length > 1)
            {
                return DeserializeUrlEncoded(query[1]);
            }
            return new Dictionary<string, string>();
        }

        private Dictionary<string, string> DeserializeUrlEncoded(string raw)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            raw = HttpUtility.UrlDecode(raw);

            string[] KVPairs = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < KVPairs.Length; index++)
            {
                string[] currentKV = KVPairs[index].Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (currentKV.Length == 2)
                {
                    result.Add(currentKV[0], currentKV[1]);
                }
            }

            return result;
        }

        private void ParseHeaders(byte[] rawheaders)
        {
            StringReader sr = new StringReader(Encoding.UTF8.GetString(rawheaders));
            string[] firstLine = sr.ReadLine().Split(' ');
            Method = HttpMethodConvert.FromString(firstLine[0]);
            Path = firstLine[1];
            Version = HttpVersionConvert.FromString(firstLine[2]);

            string currentHeader;
            while ((currentHeader = sr.ReadLine()) != null)
            {
                string[] header = currentHeader.Split(": ");
                if (header.Length != 2)
                {
                    throw new ArgumentException($"The header '{currentHeader}' is not valid.");
                }
                Headers.Add(header[0], header[1]);
            }
        }
    }
}