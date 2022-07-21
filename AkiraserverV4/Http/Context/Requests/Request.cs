using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
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
        public MemoryStream Body { get; set; } = new MemoryStream();
        public Dictionary<string, dynamic> Params { get; private set; } = new Dictionary<string, dynamic>();

        public static Request TryParseRequest(NetworkStream networkStream, RequestSettings settings)
        {
            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }

            var request = new Request();

            var buffer = ArrayPool.Rent(settings.ReadPacketSize);

            var dataRead = networkStream.Read(buffer);

            var data = buffer.AsSpan(0, dataRead);

            request.ParseFirstPacket(data);

            ArrayPool.Return(buffer);

            request.ParseUrlQuery();

            if (request.HttpHeaders is null)
            {
                return request;
            }

            long maxBodySize;

            if (request.HttpHeaders.ContainsKey(HeaderNames.ContentLength))
            {
                maxBodySize = long.Parse(request.HttpHeaders[HeaderNames.ContentLength]);
            }
            else
            {
                maxBodySize = 0;
            }

            int remeaning = (int)(maxBodySize - (request.Body.Position + 1));

            if (remeaning < 1)
            {
                return request;
            }

            if (remeaning > settings.ReadPacketSize)
            {
                remeaning = settings.ReadPacketSize;
            }

            var currentBuffer = ArrayPool.Rent(remeaning);

            dataRead = networkStream.Read(currentBuffer);

            if (dataRead < 1)
            {
                ArrayPool.Return(currentBuffer);
                // no more data available
                return request;
            }

            request.Body.Write(currentBuffer, 0, dataRead);

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

        private void ParseFirstPacket(Span<byte> data)
        {
            var headerEnding = HttpHeaders.Parse(data, ParseErrors);

            if (ParseErrors.Count > 0)
            {
                return;
            }

            if (headerEnding < data.Length)
            {
                Body.Write(data.Slice(headerEnding, data.Length - headerEnding));
            }
        }
    }
}