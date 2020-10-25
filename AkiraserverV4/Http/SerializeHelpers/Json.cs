using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AkiraserverV4.Http.Helper;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class JsonResult : ResponseResult
    {
        public JsonResult(object obj) : base(obj)
        {
            ContentType = ContentType.JSON;
        }

        public override async Task<Stream> Serialize()
        {
            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, Content).ConfigureAwait(false);
            return ms;
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