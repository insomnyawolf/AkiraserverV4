using AkiraserverV4.Http.BaseContex.Responses;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContex
{
    public class DefaultContext : Context
    {
        [DefaultBadRequestEndpoint]
        public async Task DefaultBadRequestEndpoint(Exception exception)
        {
            Response.Status = HttpStatus.InternalServerError;
            await JsonSerializer.SerializeAsync(utf8Json: NetworkStream, value: exception).ConfigureAwait(false);
        }

        [DefaultNotFoundEndpoint]
        public string DefaultNotFoundEndpoint()
        {
            Response.Status = HttpStatus.NotFound;
            return "404 NotFound";
        }

        [DefaultInternalServerErrorEndpoint]
        public async Task DefaultInternalServerErrorEndpoint(Exception exception)
        {
            Response.Status = HttpStatus.InternalServerError;
            await JsonSerializer.SerializeAsync(utf8Json: NetworkStream, value: exception).ConfigureAwait(false);
        }
    }
}