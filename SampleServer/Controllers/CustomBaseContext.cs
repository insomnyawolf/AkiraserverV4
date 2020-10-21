using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
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
    }
}