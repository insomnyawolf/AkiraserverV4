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
}