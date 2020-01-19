using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.Helper;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AkiraserverV4.Http.BaseContex.Requests;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class DefaultOverwriteContext : Context
    {
        private readonly ILogger<DefaultOverwriteContext> logger;

        public DefaultOverwriteContext(ILogger<DefaultOverwriteContext> logger)
        {
            this.logger = logger;
        }

        [NotFoundHandler]
        public string NotFoundHandler()
        {
            var data = JsonSerializer.Serialize(new RequestData(Request));
            return "Ok\n" + data;
        }

        class RequestData
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