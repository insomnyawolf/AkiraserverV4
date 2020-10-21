using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
        private static readonly char[] HeaderDelimiter = "\r\n\r\n".ToCharArray();
        public Headers Headers { get; set; }
        public MemoryStream Body { get; set; }
        public List<FormInput> UrlQuery { get; private set; }


        public static async Task<Request> BuildRequest(NetworkStream networkStream, RequestSettings settings)
        {

            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }


            byte[] currentBuffer = new byte[settings.ReadPacketSize];

            int dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer, settings.ReadPacketSize, settings.ReciveTimeout).ConfigureAwait(false);

            var temp = await ParseFirstPacket(currentBuffer, dataRead).ConfigureAwait(false);

            long? bodySize = null;

            if (temp.Headers.RequestHeaders.ContainsKey(Header.ContentLength))
            {
                bodySize = long.Parse(temp.Headers.RequestHeaders[Header.ContentLength]);
            }

            int remeaning = settings.ReadPacketSize;

            while (dataRead == settings.ReadPacketSize)
            {
#warning Check if this is worth
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

            Request req = new Request()
            {
                Headers = temp.Headers,
                Body = temp.Body,
            };

            req.ParseUrlQuery();

            return req;
        }

        private void ParseUrlQuery()
        {
            string[] query = Headers.Path.Split('?', StringSplitOptions.RemoveEmptyEntries);

            if (query.Length > 0)
            {
                Headers.Path = query[0];
            }

            if (query.Length > 1)
            {
                UrlQuery = DeserializeUrlEncoded(query[1]);
            }
        }

        private static List<FormInput> DeserializeUrlEncoded(string raw)
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

            return result;
        }

        private static async Task<RequestData> ParseFirstPacket(byte[] stream, int maxPosition)
        {


            StringBuilder headersRaw = new StringBuilder();

            char[] checkGroup = new char[HeaderDelimiter.Length];

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

                if (HeaderDelimiter.SequenceEqual(checkGroup))
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

            var headers = new Headers()
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

            MemoryStream body;

            if (position < maxPosition)
            {
                body = new MemoryStream();
                await body.WriteAsync(stream, position, maxPosition - position).ConfigureAwait(false);
            }
            else
            {
                body = null;
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