using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.Helper;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;

namespace AkiraserverV4.Http.BaseContex
{
    public class DefaultContext : BaseContext
    {
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
            await JsonSerializer.SerializeAsync(utf8Json: NetworkStream, value: exception);
        }
    }
}