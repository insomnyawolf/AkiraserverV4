using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
        private static readonly char[] HttpDelimiter = "\r\n\r\n".ToCharArray();
        private static readonly int[] HttpDelimiterBinary = HttpDelimiterBinaryInit();

        private static int[] HttpDelimiterBinaryInit()
        {
            var arr = new int[HttpDelimiter.Length];
            for (int i = 0; i < HttpDelimiter.Length; i++)
            {
                arr[i] = HttpDelimiter[i];
            }
            return arr;
        }

        public Header Header { get; set; }
#if DEBUG
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public MemoryStream Body { get; set; }
        public Dictionary<string, dynamic> Params { get; private set; } = new Dictionary<string, dynamic>();

        public static async Task<Request> BuildRequest(NetworkStream networkStream, RequestSettings settings, Request req = null)
        {
            if (req is null)
            {
                req = new Request();
            }

            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }


            byte[] currentBuffer = new byte[settings.ReadPacketSize];

            int dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer, settings.ReadPacketSize, settings.ReciveTimeout).ConfigureAwait(false);

            var temp = await ParseFirstPacket(currentBuffer, dataRead).ConfigureAwait(false);

            long? bodySize = null;

            if (temp.Headers.RequestHeaders.ContainsKey(Context.HeaderNames.ContentLength))
            {
                bodySize = long.Parse(temp.Headers.RequestHeaders[Context.HeaderNames.ContentLength]);
            }

            int remeaning = settings.ReadPacketSize;

            while (remeaning > 0 && dataRead > 0)
            {
                if (bodySize.HasValue)
                {
                    remeaning = (int)(bodySize.Value - temp.Body.Position);

                    if (remeaning > settings.ReadPacketSize)
                    {
                        remeaning = settings.ReadPacketSize;
                    }
                }

                dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer, remeaning, settings.ReciveTimeout).ConfigureAwait(false);

                if (dataRead > 0)
                {
                    await temp.Body.WriteAsync(currentBuffer, 0, dataRead).ConfigureAwait(false);
                }
            }

            req.Header = temp.Headers;
            req.Body = temp.Body;

            req.ParseUrlQuery();

            return req;
        }

        private void ParseUrlQuery()
        {
            string[] query = Header.Path.Split('?', StringSplitOptions.RemoveEmptyEntries);

            if (query.Length > 0)
            {
                Header.Path = query[0];
            }

            if (query.Length > 1)
            {
                DeserializeUrlEncodedQuery(query[1]);
            }
        }

        private void DeserializeUrlEncodedQuery(string raw)
        {
            raw = HttpUtility.UrlDecode(raw);

            string[] KVPairs = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < KVPairs.Length; index++)
            {
                string[] currentKV = KVPairs[index].Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (currentKV.Length == 2)
                {
                    Params.Add(currentKV[0], currentKV[1]);
                }
            }
        }

        public async Task DeserializeUrlEncodedBody()
        {
            DeserializeUrlEncodedQuery(await ReadStringPayloadAsync().ConfigureAwait(false));
        }

        private static async Task<RequestData> ParseFirstPacket(byte[] stream, int maxPosition)
        {


            StringBuilder headersRaw = new StringBuilder();

            char[] checkGroup = new char[HttpDelimiter.Length];

            int position = 0;
            while (position < maxPosition)
            {
                for (int i = 1; i < checkGroup.Length; i++)
                {
                    checkGroup[i - 1] = checkGroup[i];
                }

                char currentChar = (char)stream[position];
                headersRaw.Append(currentChar);
                checkGroup[^1] = currentChar;

                position++;

                if (HttpDelimiter.PatternEquals(checkGroup))
                {
                    break;
                }
            }

            var RequestReader = new StringReader(headersRaw.ToString());

            string data = await RequestReader.ReadLineAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(data))
            {
                throw new MalformedRequestException();
            }

            string[] firstLine = data.Split(' ');

            if (firstLine.Length != 3)
            {
                throw new MalformedRequestException();
            }

            var headers = new Header()
            {
                Method = HttpMethodConvert.FromString(firstLine[0]),
                Path = firstLine[1],
                Version = HttpVersionConvert.FromString(firstLine[2]),
                RequestHeaders = new Dictionary<string, string>()
            };

            string currentHeader;
            while (!string.IsNullOrWhiteSpace(currentHeader = RequestReader.ReadLine()))
            {
                string[] header = currentHeader.Split(": ");
                if (header.Length != 2)
                {
                    throw new MalformedRequestException();
                }
                headers.RequestHeaders.Add(header[0], header[1]);
            }

            MemoryStream body = new MemoryStream();

            if (position < maxPosition)
            {
                await body.WriteAsync(stream, position, maxPosition - position).ConfigureAwait(false);
            }

            var requestData = new RequestData()
            {
                Body = body,
                Headers = headers
            };

            return requestData;
        }
    }
}