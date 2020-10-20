using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class XmlResult : ResponseResult
    {
        public XmlResult(object obj) : base(obj)
        {
        }

        public override string Serialize()
        {
            using var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(Content.GetType());
            serializer.Serialize(stringwriter, Content);
            return stringwriter.ToString();
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