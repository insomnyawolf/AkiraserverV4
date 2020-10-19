using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task<Stream> ReadRawPayload()
        {
#warning To Implement In A Decent Way
            return NetworkStream;
        }

        public async Task<string> ReadStringPayload()
        {
            return await new StreamReader(NetworkStream).ReadToEndAsync().ConfigureAwait(false);
        }

        public async Task<T> ReadJsonPayload<T>()
        {
            string data = await ReadStringPayload().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(data);
        }

        public T ReadXmlPayload<T>()
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(NetworkStream);
        }

        public async Task<Form> ReadUrlEncodedPayload()
        {
            string data = await ReadStringPayload().ConfigureAwait(false);
            return DeserializeUrlEncoded(data);
        }

        public async Task ReadMultipartPayload()
        {
#warning To Implement This
        }
    }
}