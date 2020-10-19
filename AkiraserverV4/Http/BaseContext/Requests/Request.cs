using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public partial class Request
    {
        private static readonly byte[] HeaderSeparator = Encoding.UTF8.GetBytes("\r\n\r\n");

        public NetworkStream NetworkStream { get; private set; }
        public HttpMethod Method { get; private set; }
        public string Path { get; private set; }
        public HttpVersion Version { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public Form UrlQuery { get; private set; }

        private Request(NetworkStream networkStream)
        {
            NetworkStream = networkStream;
        }

        public static async Task<Request> ParseRequest(NetworkStream networkStream)
        {
            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }

            Request request = new Request(networkStream);

            await request.ParseHeaders(networkStream).ConfigureAwait(false);

            request.ParseUrlQuery();

            return request;
        }

        private void ParseUrlQuery()
        {
            string[] query = Path.Split('?', StringSplitOptions.RemoveEmptyEntries);

            if (query.Length > 0)
            {
                Path = query[0];
            }

            if (query.Length > 1)
            {
                UrlQuery = DeserializeUrlEncoded(query[1]);
            }
        }

        private static Form DeserializeUrlEncoded(string raw)
        {
            List<FormInput> result = new List<FormInput>();

            raw = HttpUtility.UrlDecode(raw);

            string[] KVPairs = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < KVPairs.Length; index++)
            {
                string[] currentKV = KVPairs[index].Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (currentKV.Length == 2)
                {
                    result.Add(new FormInput() 
                    {
                        Name = currentKV[0],
                        Value = currentKV[1]
                    });
                }
            }

            return new Form()
            {
                FormInput = result
            };
        }

        private async Task  ParseHeaders(NetworkStream networkStream)
        {
            var reader = new StreamReader(networkStream);

            string data = await reader.ReadLineAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(data))
            {
                throw new MalformedRequestException();
            }

            string[] firstLine = data.Split(' ');

            if (firstLine.Length != 3)
            {
                throw new MalformedRequestException();
            }

            Method = HttpMethodConvert.FromString(firstLine[0]);
            Path = firstLine[1];
            Version = HttpVersionConvert.FromString(firstLine[2]);

            Headers = new Dictionary<string, string>();

            string currentHeader;
            while (!string.IsNullOrWhiteSpace(currentHeader = reader.ReadLine()))
            {
                string[] header = currentHeader.Split(": ");
                if (header.Length != 2)
                {
                    throw new MalformedRequestException();
                }
                Headers.Add(header[0], header[1]);
            }
        }
    }
}