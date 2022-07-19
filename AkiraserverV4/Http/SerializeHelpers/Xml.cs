using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class XmlResult : ResponseResult
    {
        public override ContentType ContentType { get; set; } = ContentType.XML;
        public dynamic Value { get; set; }

        public XmlResult(dynamic Value)
        {
            this.Value = Value;
        }

        public override Task SerializeToNetworkStream(Response Response)
        {
            if (Value is null)
            {
                Response.StreamWriter.Write("null");
                return Task.CompletedTask;
            }

            var serializer = new XmlSerializer(Value.GetType());
            serializer.Serialize(Response.NetworkStream, Value);

            return Task.CompletedTask;
        }
    }

    public class XmlDeserialize
    {
        public const string ContentType = "application/xml";

        public static object DeSerialize(Type type, Stream body)
        {
            return new XmlSerializer(type).Deserialize(new StreamReader(body));
        }
    }
}