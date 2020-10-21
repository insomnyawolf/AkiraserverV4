using System;
using System.IO;
using System.Text.Json;
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

        public override string Serialize()
        {
            using var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(Content?.GetType() ?? TypeOfObject);
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