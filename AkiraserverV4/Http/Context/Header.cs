using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AkiraserverV4.Http.Context
{
    public class HttpHeaders : Dictionary<string, string>
    {
        public const string HeaderSeparator = "\r\n";
        private static readonly char[] HttpDelimiter = "\r\n\r\n".ToCharArray();

        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion ProtocolVersion { get; set; }

        public int Parse(Span<byte> data, List<string> ParseErrors)
        {
            int headerEnding = 0;

            var headersRaw = new StringBuilder();

            char[] checkGroup = new char[HttpDelimiter.Length];

            for (int indexData = 0; indexData < data.Length; indexData++)
            {
                for (int i = 1; i < checkGroup.Length; i++)
                {
                    checkGroup[i - 1] = checkGroup[i];
                }

                char currentChar = (char)data[indexData];

                checkGroup[^1] = currentChar;

                headersRaw.Append(currentChar);

                if (HttpDelimiter.PatternEquals(checkGroup))
                {
                    headerEnding = indexData + 1;
                    break;
                }
            }

            var RequestReader = new StringReader(headersRaw.ToString());

            string dataString = RequestReader.ReadLine();

            if (string.IsNullOrEmpty(dataString))
            {
                ParseErrors.Add("Invalid request, no headers were provided.");
            }

            string[] firstLine = dataString.Split(' ');

            if (firstLine.Length != 3)
            {
                ParseErrors.Add("First header where verb, path and http version should be is invalid.");
            }

            Method = HttpMethodConvert.FromString(firstLine[0]);
            Path = firstLine[1];
            ProtocolVersion = HttpVersionConvert.FromString(firstLine[2]);

            string currentHeader;
            while (!string.IsNullOrWhiteSpace(currentHeader = RequestReader.ReadLine()))
            {
                string[] header = currentHeader.Split(": ");
                if (header.Length != 2)
                {
                    ParseErrors.Add($"The header: '{currentHeader}' contain more than 2 parts.");
                    continue;
                }
                Add(header[0], header[1]);
            }

            return headerEnding;
        }
    }

    public class HttpResponseHeaders : HttpHeaders
    {
        public HttpStatus Status { get; set; } = HttpStatus.Unset;

        public string Serialize()
        {
            StringBuilder headerBuilder = new StringBuilder();
            headerBuilder.Append(ProtocolVersion.ToVersionString());
            headerBuilder.Append(' ');
            headerBuilder.Append(Status.ToStatusString());
            headerBuilder.Append(HeaderSeparator);

            foreach (KeyValuePair<string, string> header in this)
            {
                string key = header.Key;
                string value = header.Value;

                if (key == HeaderNames.ContentLength && Status == HttpStatus.NoContent)
                {
                    value = "0";
                }

                headerBuilder.Append(key);
                headerBuilder.Append(": ");
                headerBuilder.Append(value);
                headerBuilder.Append(HeaderSeparator);
            }

            headerBuilder.Append(HeaderSeparator);

            return headerBuilder.ToString();
        }
    }
}
