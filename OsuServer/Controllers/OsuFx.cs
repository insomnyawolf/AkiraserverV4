using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
using Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class OsuFxContext : Context
    {
        private readonly ILogger<DefaultOverwriteContext> logger;

        public OsuFxContext(ILogger<DefaultOverwriteContext> logger)
        {
            this.logger = logger;
        }

        [Get("/v10/servers")]
        public string ListServers()
        {
            var server = new OsuFxServer()
            {
                Avatar = "localhost/avatar",
                Bancho = "localhost/bancho",
                Direct = "localhost/direct",
                DirectAuth = 0,
                Frontend = "localhost/frontend",
                Host = "localhost",
                Protocol = "http://"
            };
            var data = JsonSerializer.Serialize(server.AsArray());
            return data;
        }

        public partial class OsuFxServer
        {
            [JsonPropertyName("DirectAuth")]
            public long DirectAuth { get; set; }

            [JsonPropertyName("Direct")]
            public string Direct { get; set; }

            [JsonPropertyName("Avatar")]
            public string Avatar { get; set; }

            [JsonPropertyName("protocol")]
            public string Protocol { get; set; }

            [JsonPropertyName("Frontend")]
            public string Frontend { get; set; }

            [JsonPropertyName("Bancho")]
            public string Bancho { get; set; }

            [JsonPropertyName("Host")]
            public string Host { get; set; }
        }

        private class RequestData
        {
            public string Path { get; set; }
            public string Method { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public string Data { get; set; }

            public RequestData(Request raw)
            {
                Path = raw.Path;
                Method = raw.Method.ToString();
                Headers = raw.Headers;
                Data = raw.Body.ToString();
            }
        }
    }
}