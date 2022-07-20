#if DEBUG
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AkiraserverV4.Http.Context.Requests
{
    public partial class Request
    {
        internal void LogPacket(ILogger<Request> Logger)
        {
            var test = JsonSerializer.Serialize(this, new JsonSerializerOptions() 
            { 
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter() 
                }
            });
            Logger.LogInformation(test);
        }
    } 
}
#endif