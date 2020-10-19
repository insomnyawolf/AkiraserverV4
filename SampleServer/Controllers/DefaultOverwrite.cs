using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace SampleServer
{
    [Controller("/[controller]")]
    public class CustomBaseContext : BaseContext
    {
        private readonly ILogger<CustomBaseContext> Logger;

        public CustomBaseContext()
        {
            Logger = Program.ServiceProvider.GetRequiredService<ILogger<CustomBaseContext>>();
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
            }
        }
    }
}