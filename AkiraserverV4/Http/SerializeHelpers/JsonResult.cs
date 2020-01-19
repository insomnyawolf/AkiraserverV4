using System.Text.Json;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public class JsonResult
    {
        public object Content { get; set; }

        public string SerializedJson => JsonSerializer.Serialize(Content);

        public JsonResult(object obj)
        {
            Content = obj;
        }
    }
}