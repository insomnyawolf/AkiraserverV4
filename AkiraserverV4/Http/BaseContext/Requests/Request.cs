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

        public List<string> ParseErrors { get; } = new List<string>();

        public RequestHeaders RequestHeaders { get; set; }
#if DEBUG
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public MemoryStream Body { get; set; }
        public Dictionary<string, dynamic> Params { get; private set; } = new Dictionary<string, dynamic>();

        public static async Task<Request> TryParseRequest(NetworkStream networkStream, RequestSettings settings)
        {
            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }

            var request = new Request();

            byte[] currentBuffer = new byte[settings.ReadPacketSize];

            int dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer, settings.ReadPacketSize, settings.ReciveTimeout).ConfigureAwait(false);

            await request.ParseFirstPacket(currentBuffer, dataRead).ConfigureAwait(false);

            if (request.RequestHeaders is null)
            {
                return request;
            }

            long? bodySize = null;

            if (request.RequestHeaders.ContainsKey(HeaderNames.ContentLength))
            {
                bodySize = long.Parse(request.RequestHeaders[HeaderNames.ContentLength]);
            }

            int remeaning = settings.ReadPacketSize;

            while (remeaning > 0 && dataRead > 0)
            {
                if (bodySize.HasValue)
                {
                    remeaning = (int)(bodySize.Value - request.Body.Position);

                    if (remeaning > settings.ReadPacketSize)
                    {
                        remeaning = settings.ReadPacketSize;
                    }
                }

                dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer, remeaning, settings.ReciveTimeout).ConfigureAwait(false);

                if (dataRead > 0)
                {
                    await request.Body.WriteAsync(currentBuffer, 0, dataRead).ConfigureAwait(false);
                }
            }

            request.ParseUrlQuery();

            return request;
        }

        private void ParseUrlQuery()
        {
            string[] query = RequestHeaders.Path.Split('?', StringSplitOptions.RemoveEmptyEntries);

            if (query.Length > 0)
            {
                RequestHeaders.Path = query[0];
            }

            if (query.Length > 1)
            {
                DeserializeUrlEncodedQuery(query[1]);
            }
        }

        private void DeserializeUrlEncodedQuery(string raw)
        {
            string[] KVPairs = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < KVPairs.Length; index++)
            {
                string[] currentKV = KVPairs[index].Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (currentKV.Length == 2)
                {
                    Params.Add(HttpUtility.UrlDecode(currentKV[0]), HttpUtility.UrlDecode(currentKV[1]));
                }
            }
        }

        public async Task DeserializeUrlEncodedBody()
        {
            DeserializeUrlEncodedQuery(await ReadStringPayloadAsync().ConfigureAwait(false));
        }

        private async Task ParseFirstPacket(byte[] stream, int maxPosition)
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
                ParseErrors.Add("Invalid request, no headers were provided.");
                return;
            }

            string[] firstLine = data.Split(' ');

            if (firstLine.Length != 3)
            {
                ParseErrors.Add("First header where verb, path and http version should be is invalid.");
                return;
            }

            RequestHeaders = new RequestHeaders()
            {
                Method = HttpMethodConvert.FromString(firstLine[0]),
                Path = firstLine[1],
                Version = HttpVersionConvert.FromString(firstLine[2]),
            };

            string currentHeader;
            while (!string.IsNullOrWhiteSpace(currentHeader = RequestReader.ReadLine()))
            {
                string[] header = currentHeader.Split(": ");
                if (header.Length != 2)
                {
                    ParseErrors.Add($"The header: '{currentHeader}' contain more than 2 parts.");
                    return;
                }
                RequestHeaders.Add(header[0], header[1]);
            }

            Body = new MemoryStream();

            if (position < maxPosition)
            {
                await Body.WriteAsync(stream, position, maxPosition - position).ConfigureAwait(false);
            }
        }
    }
}