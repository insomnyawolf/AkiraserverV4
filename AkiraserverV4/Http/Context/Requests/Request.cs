using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
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

        public static async Task<Request> TryParseRequest(BufferedStream networkStream, RequestSettings settings)
        {
            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }

            var request = new Request();

            byte[] currentBuffer = new byte[settings.ReadPacketSize];

            int dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer);

            Array.Resize(ref currentBuffer, dataRead);

            request.ParseFirstPacket(currentBuffer);

            if (request.HttpHeaders is null)
            {
                return request;
            }

            long? bodySize = null;

            if (request.HttpHeaders.ContainsKey(HeaderNames.ContentLength))
            {
                bodySize = long.Parse(request.HttpHeaders[HeaderNames.ContentLength]);
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

                dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer);

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

        private void ParseFirstPacket(byte[] data)
        {
            var headerEnding = HttpHeaders.Parse(data, ParseErrors);

            if (ParseErrors.Count > 0)
            {
                return;
            }

            Body = new MemoryStream();

            if (headerEnding < data.Length)
            {
                Body.Write(data, headerEnding, data.Length - headerEnding);
            }
        }
    }
}