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
        public MemoryStream RequestStream { get; private set; }
        private long? BodyBegginingPosition;
        public Headers Headers { get; set; }
        public Form UrlQuery { get; private set; }


        public static async Task<Request> BuildRequest(NetworkStream networkStream, RequestSettings settings)
        {

            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }



            var RequestStream = new MemoryStream();
            byte[] currentBuffer = new byte[settings.ReadPacketSize];

            int dataRead;
            while (true)
            {
                dataRead = await networkStream.ReadAsyncWithTimeout(currentBuffer, settings.ReciveTimeout).ConfigureAwait(false);
                await RequestStream.WriteAsync(currentBuffer, 0, dataRead).ConfigureAwait(false);
                if (dataRead != currentBuffer.Length)
                {
                    break;
                }
            }

            var temp = await ParseHeaders(RequestStream).ConfigureAwait(false);

            Request req = new Request()
            {
                Headers = temp.Headers,
                BodyBegginingPosition = temp.BodyBeginningPosition,
                RequestStream = RequestStream,
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

        private static async Task<RequestData> ParseHeaders(Stream stream)
        {
            stream.Position = 0;

            var requestData = new RequestData();

            StringBuilder headersRaw = new StringBuilder();

            char[] checkGroup = new char[HeaderDelimiter.Length];

            // ReadByte - we're working with binary file...
            while (stream.Position < stream.Length)
            {
                for (int i = 1; i < checkGroup.Length; i++)
                {
                    checkGroup[i - 1] = checkGroup[i];
                }

                char currentChar = (char)stream.ReadByte();
                headersRaw.Append(currentChar);
                checkGroup[^1] = currentChar;

                if (HeaderDelimiter.SequenceEqual(checkGroup))
                {
                    requestData.BodyBeginningPosition = stream.Position;
                    break;
                }
            }

            stream.Position = 0;

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

            requestData.Headers = headers;

            return requestData;
        }
    }
}