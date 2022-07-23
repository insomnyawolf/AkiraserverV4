using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AkiraserverV4.Http.Context
{
    public class HttpHeaders : Dictionary<string, string>
    {
        public const string HeaderSeparator = "\r\n";
        private static readonly char[] HttpDelimiter = "\r\n\r\n".ToCharArray();
        private static readonly int CheckGroupLastPosition = HttpDelimiter.Length - 1;

        private static readonly DefaultObjectPoolProvider DefaultObjectPoolProvider = new DefaultObjectPoolProvider();
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = DefaultObjectPoolProvider.CreateStringBuilderPool();

        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion ProtocolVersion { get; set; }

        public int Parse(Span<byte> data, List<string> ParseErrors)
        {
            int headerEnding = 0;

            var headersRaw = StringBuilderPool.Get();

            char[] checkGroup = new char[HttpDelimiter.Length];

            for (int indexData = 0; indexData < data.Length; indexData++)
            {
                for (int i = 1; i < checkGroup.Length; i++)
                {
                    checkGroup[i - 1] = checkGroup[i];
                }

                char currentChar = (char)data[indexData];

                checkGroup[CheckGroupLastPosition] = currentChar;

                headersRaw.Append(currentChar);

                if (HttpDelimiter.PatternEquals(checkGroup))
                {
                    headerEnding = indexData + 1;
                    break;
                }
            }

            var headers = headersRaw.ToString().Split(HeaderSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (headers.Length < 1)
            {
                ParseErrors.Add("Invalid request, no headers were provided.");
            }

            if (headers.Length < 1)
            {
                ParseErrors.Add("First header where verb, path and http version should be is invalid.");
                return -1;
            }

            string[] firstLine = headers[0].Split(' ');

            if (firstLine.Length != 3)
            {
                ParseErrors.Add("First header where verb, path and http version should be is invalid.");
            }

            Method = HttpMethodConvert.FromString(firstLine[0]);
            Path = firstLine[1];
            ProtocolVersion = HttpVersionConvert.FromString(firstLine[2]);

            for (int index = 1; index < headers.Length; index++)
            {
                var current = headers[index];

                string[] header = current.Split(": ");
                if (header.Length != 2)
                {
                    ParseErrors.Add($"The header: '{current}' contain more than 2 parts.");
                    continue;
                }

                Add(header[0], header[1]);
            }

            StringBuilderPool.Return(headersRaw);

            return headerEnding;
        }
    }

    public class HttpResponseHeaders : HttpHeaders
    {
        public HttpStatus Status { get; set; } = HttpStatus.Unset;

        public string Serialize()
        {
            var headerBuilder = new StringBuilder();
            headerBuilder.Append(ProtocolVersion.ToVersionString());
            headerBuilder.Append(' ');
            headerBuilder.Append(Status.ToStatusString());
            headerBuilder.Append(HeaderSeparator);

            if (Status == HttpStatus.NoContent)
            {
                headerBuilder.Append(HttpHeaderNames.ContentLength);
                headerBuilder.Append(':').Append(' ');
                headerBuilder.Append('0');
            }

            foreach (KeyValuePair<string, string> header in this)
            {
                string key = header.Key;
                string value = header.Value;

                headerBuilder.Append(key);
                headerBuilder.Append(':').Append(' ');
                headerBuilder.Append(value);
                headerBuilder.Append(HeaderSeparator);
            }

            headerBuilder.Append(HeaderSeparator);

            return headerBuilder.ToString();
        }

        public HttpResponseHeaders DeepClone()
        {
            var headers = new HttpResponseHeaders();

            foreach (var header in this)
            {
                headers.Add(header.Key, header.Value);
            }

            return headers;
        }
    }
}
