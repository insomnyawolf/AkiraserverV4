using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public partial class Request
    {
        private const string HeaderDelimiter = "\r\n\r\n";
        private MemoryStream ReadRawPayload()
        {
            RequestStream.Position = 0;
            RequestStream.Position = SeekToDelimiter(RequestStream, HeaderDelimiter);
            return RequestStream;
        }

        private static long SeekToDelimiter(MemoryStream stream, string delimiter)
        {
            char[] HeaderDelimiterBytes = delimiter.ToCharArray();
            char[] checkGroup = new char[HeaderDelimiterBytes.Length];

            // ReadByte - we're working with binary file...
            while (stream.Position < stream.Length)
            {
                for (int i = 1; i < checkGroup.Length; i++)
                {
                    checkGroup[i - 1] = checkGroup[i];
                }

                checkGroup[^1] = (char)stream.ReadByte();

                if (HeaderDelimiterBytes.SequenceEqual(checkGroup))
                {
                    return stream.Position;
                }
            }
            return -1;
        }

        public string ReadStringPayload()
        {
            var war = ReadRawPayload();

            return new StreamReader(war).ReadToEnd();
        }

        public T ReadJsonPayload<T>()
        {
            string data = ReadStringPayload();
            return JsonSerializer.Deserialize<T>(data);
        }

        public T ReadXmlPayload<T>()
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(NetworkStream);
        }

        public Form ReadUrlEncodedPayload()
        {
            string data = ReadStringPayload();
            return DeserializeUrlEncoded(data);
        }

        public async Task ReadMultipartPayload()
        {
#warning To Implement This
        }
    }
}