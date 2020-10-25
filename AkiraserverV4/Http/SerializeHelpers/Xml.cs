using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AkiraserverV4.Http.Helper;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class XmlResult : ResponseResult
    {
        private static readonly Type TypeOfObject = typeof(object);
        public XmlResult(object obj) : base(obj)
        {
            ContentType = ContentType.XML;
        }

        public override async Task<Stream> Serialize()
        {
            var serializer = new XmlSerializer(Content?.GetType() ?? TypeOfObject);
            var ms = new MemoryStream();
            serializer.Serialize(ms, Content);
            return ms;
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