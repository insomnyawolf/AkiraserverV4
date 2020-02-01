using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public class Request
    {
        private const int DefaultSize = 8192;

        private static readonly byte[] HeaderSeparator = Encoding.UTF8.GetBytes("\r\n\r\n");

        public NetworkStream NetworkStream { get; private set; }
        public bool AllRequestReaded { get; private set; }
        public HttpMethod Method { get; private set; }
        public string Path { get; private set; }
        public HttpVersion Version { get; private set; }
        public Dictionary<string, string> UrlQuery { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public List<byte> Body { get; }

        private Request(NetworkStream networkStream)
        {
            NetworkStream = networkStream;
            Body = new List<byte>();
        }

        public static async Task<Request> ParseRequest(NetworkStream networkStream)
        {
            if (networkStream is null)
            {
                throw new ArgumentNullException(nameof(networkStream));
            }

            Request request = new Request(networkStream);

            byte[] firstBuffer = new byte[DefaultSize];
            int dataRead = await networkStream.ReadPacketAsync(firstBuffer, DefaultSize).ConfigureAwait(false);

            if (dataRead == -1)
            {
                return null;
            }

            if (dataRead != DefaultSize)
            {
                byte[] partialBuffer = new byte[dataRead];
                Buffer.BlockCopy(firstBuffer, 0, partialBuffer, 0, dataRead);

                firstBuffer = partialBuffer;
                request.AllRequestReaded = true;
            }

            List<byte[]> requestParts = firstBuffer.Separate(HeaderSeparator, 1);

            if (requestParts.Count > 0)
            {
                request.Headers = new Dictionary<string, string>();
                if (!request.ParseHeaders(requestParts[0]))
                {
                    return null;
                }
            }

            request.ParseUrlQuery();

            if (requestParts.Count > 1)
            {
                request.Body.AddRange(requestParts[1]);
            }

            return request;
        }

        private async Task ReadRestOfRequest()
        {
            byte[] currentBuffer = new byte[DefaultSize];

            int dataRead;
            while ((dataRead = await NetworkStream.ReadPacketAsync(currentBuffer, DefaultSize).ConfigureAwait(false)) > 0)
            {
                if (dataRead == DefaultSize)
                {
                    Body.AddRange(currentBuffer);
                }
                else
                {
                    byte[] partialBuffer = new byte[dataRead];
                    Buffer.BlockCopy(currentBuffer, 0, partialBuffer, 0, dataRead);
                    Body.AddRange(partialBuffer);
                }
            }
        }

        public async Task<Dictionary<string, string>> GetUrlEncodedForm()
        {
            await ReadRestOfRequest().ConfigureAwait(false);
            string rawBody = Encoding.UTF8.GetString(Body.ToArray());
            return DeserializeUrlEncoded(rawBody);
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

        private bool ParseHeaders(byte[] rawheaders)
        {
            StringReader sr = new StringReader(Encoding.UTF8.GetString(rawheaders));
            string temp = sr.ReadLine();

            if (temp is null)
            {
                return false;
            }

            string[] firstLine = temp.Split(' ');

            if (firstLine.Length != 3)
            {
                return false;
            }

            Method = HttpMethodConvert.FromString(firstLine[0]);
            Path = firstLine[1];
            Version = HttpVersionConvert.FromString(firstLine[2]);

            string currentHeader;
            while ((currentHeader = sr.ReadLine()) != null)
            {
                string[] header = currentHeader.Split(": ");
                if (header.Length != 2)
                {
                    return false;
                }
                Headers.Add(header[0], header[1]);
            }
            return true;
        }
    }
}