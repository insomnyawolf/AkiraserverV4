using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Model;
using System;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private ExecutedCommand RequestedEndpoint(Request request)
        {
            for (int index = 0; index < Endpoints.Length; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.Method == request.Headers.Method && request.Headers.Path.Equals(currentEndpoint.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return currentEndpoint;
                }
            }
            return null;
        }
    }
}