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
        private MemoryStream ReadRawPayload()
        {
            if (BodyBegginingPosition == null)
            {
                BodyBegginingPosition = FindBodyBegginingPosition(RequestStream);
            }
            if (BodyBegginingPosition < 0)
            {
                return null;
            }

            RequestStream.Position = BodyBegginingPosition.Value;
            return RequestStream;
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
            return (T)new XmlSerializer(typeof(T)).Deserialize(ReadRawPayload());
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