using AkiraserverV4.Http.BaseContex.Requests;
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

                if (currentEndpoint.Method == request.Method && request.Path.Equals(currentEndpoint.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return currentEndpoint;
                }
            }
            return null;
        }
    }
}