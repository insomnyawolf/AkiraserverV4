using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.BaseContex;
using Microsoft.Extensions.Logging;
using System.IO;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private ExecutedCommand RequestedEndpoint(Request request)
        {
            for (int index = 0; index < Endpoints.Length; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.Method == request.Method && request.Path.Equals(currentEndpoint.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return currentEndpoint;
                }
            }
            return null;
        }
    }
}