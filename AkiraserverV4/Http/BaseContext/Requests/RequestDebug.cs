using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace AkiraserverV4.Http.Context.Requests
{
#if DEBUG
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
#endif
}