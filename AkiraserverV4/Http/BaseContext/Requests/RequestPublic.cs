using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
        public MemoryStream ReadRawPayload()
        {
            Body.Position = 0;
            return Body;
        }

        public async Task<string> ReadStringPayload()
        {
            var war = ReadRawPayload();

            return await new StreamReader(war).ReadToEndAsync().ConfigureAwait(false);
        }

        public async Task<T> ReadJsonPayload<T>()
        {
            return (T)await ReadJsonPayload(typeof(T)).ConfigureAwait(false);
        }

        public async Task<object> ReadJsonPayload(Type type)
        {
            string data = await ReadStringPayload().ConfigureAwait(false);
            return JsonSerializer.Deserialize(data, type);
        }

        public T ReadXmlPayload<T>()
        {
            return (T)ReadXmlPayload(typeof(T));
        }

        public object ReadXmlPayload(Type type)
        {
            return XmlDeserialize.DeSerialize(type, ReadRawPayload());
        }

        public async Task<List<FormInput>> ReadUrlEncodedPayload()
        {
            return DeserializeUrlEncoded(await ReadStringPayload().ConfigureAwait(false));
        }

        public async Task<Form> ReadMultipartPayload()
        {
#warning To Implement This
            throw new NotImplementedException();
        }
    }
}