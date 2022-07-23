using System;
using System.Text.Json;
using System.Threading.Tasks;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class JsonResult : ResponseResult
    {
        public override ContentType ContentType { get; set; } = ContentType.JSON;
        public dynamic Value { get; set; }
        public JsonResult(dynamic Value)
        {
            this.Value = Value;
        }

        public override async Task SerializeToNetworkStream(Response Response)
        {
            await JsonSerializer.SerializeAsync(Response.NetworkStream, Value).ConfigureAwait(false);
        }
    }

    public class JsonDeserialize
    {
        public const string ContentType = "application/json";

        public static object DeSerialize(Type type, string data)
        {
            return JsonSerializer.Deserialize(data, type);
        }

        public static T DeSerialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}