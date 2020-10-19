using System;
using System.Text.Json;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class JsonResult : ResponseResult
    {
        public JsonResult(object obj) : base(obj)
        {
        }

        public override string Serialize()
        {
            return JsonSerializer.Serialize(Content);
        }
    }

    public class JsonDeserialize
    {
        public const string ContentType = "application/json";

        public static object DeSerialize(Type type, string data)
        {
            return JsonSerializer.Deserialize(data, type);
        }
    }
}