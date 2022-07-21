using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Model;
using System;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private ExecutedCommand GetEndpoint(Request request)
        {
            if (request is null)
            {
                return null;
            }

            for (int index = 0; index < Endpoints.Count; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.Method == request.HttpHeaders.Method && request.HttpHeaders.Path.Equals(currentEndpoint.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return currentEndpoint;
                }
            }
            return null;
        }

        private ExecutedCommand GetEndpoint(SpecialEndpoint specialEndpoint)
        {
            for (int index = 0; index < Endpoints.Count; index++)
            {
                Endpoint currentEndpoint = Endpoints[index];

                if (currentEndpoint.SpecialEndpoint == specialEndpoint)
                {
                    return currentEndpoint;
                }
            }
            return null;
        }
    }
}