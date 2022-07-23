using AkiraserverV4.Http.Helper;
using Microsoft.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
        private static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new();
        private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

        //private static readonly int[] HttpDelimiterBinary = HttpDelimiterBinaryInit();

        //private static int[] HttpDelimiterBinaryInit()
        //{
        //    var arr = new int[HttpDelimiter.Length];
        //    for (int i = 0; i < HttpDelimiter.Length; i++)
        //    {
        //        arr[i] = HttpDelimiter[i];
        //    }
        //    return arr;
        //}

        public List<string> ParseErrors { get; } = new List<string>();

        public HttpHeaders HttpHeaders { get; set; } = new HttpHeaders();
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

            var buffer = ArrayPool.Rent(settings.ReadPacketSize);

            var dataRead = await networkStream.ReadAsync(buffer);

            var headersEnding = ParseHeaders();

            int ParseHeaders()
            {
                // SpanWorkaround
                var data = buffer.AsSpan(0, dataRead);

                return request.HttpHeaders.Parse(data, request.ParseErrors);
            }

            if (request.ParseErrors.Count > 0)
            {
                ArrayPool.Return(buffer);
                return request;
            }

            request.ParseUrlQuery();

            if (!request.HttpHeaders.TryGetValue(HttpHeaderNames.ContentLength, out var contentLengthString))
            {
                ArrayPool.Return(buffer);
                // if there's no body there's nothing else to parse
                return request;
            }

            var contentLength = long.Parse(contentLengthString);

            if (headersEnding < dataRead)
            {
                var length = dataRead - headersEnding;
                request.Body = RecyclableMemoryStreamManager.GetStream();
                await request.Body.WriteAsync(buffer, headersEnding, length);
            }

            ArrayPool.Return(buffer);

            int remeaningContentLength = (int)(contentLength - (request.Body.Position + 1));

            if (remeaningContentLength < 1)
            {
                //everything was read already;
                return request;
            }

            if (remeaningContentLength > settings.ReadPacketSize)
            {
                throw new IndexOutOfRangeException("The body of the request exceeds the maximum configured body size");
                //remeaning = settings.ReadPacketSize;
            }

            var currentBuffer = ArrayPool.Rent(remeaningContentLength);

            dataRead = await networkStream.ReadAsync(currentBuffer);

            if (dataRead < 1)
            {
                ArrayPool.Return(currentBuffer);
                // no more data available
                return request;
            }

            await request.Body.WriteAsync(currentBuffer, 0, dataRead);

            ArrayPool.Return(currentBuffer);

            return request;
        }

        private void ParseUrlQuery()
        {
            string[] query = HttpHeaders.Path.Split('?', StringSplitOptions.RemoveEmptyEntries);

            if (query.Length > 0)
            {
                HttpHeaders.Path = query[0];
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
    }
}