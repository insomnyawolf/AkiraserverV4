using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

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
            logger.LogInformation("Not Found:", new RequestData(Request));
            return "OverWritten Not Found Handler";
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