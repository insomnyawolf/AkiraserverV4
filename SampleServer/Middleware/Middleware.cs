using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.BaseContext.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleServer.Middleware
{
    public class Middleware : BaseContext
    {
        private static readonly ILogger<Middleware> Logger = Program.ServiceProvider.GetRequiredService<ILogger<Middleware>>();
        //public override string Execute()
        //{
        //}
        public override async Task<object> BadRequest(Exception exception)
        {
            Response.Status = HttpStatus.BadRequest;
            return exception;
        }

        public override async Task<object> NotFound(Request request)
        {
            Response.Status = HttpStatus.NotFound;
            return "Overriden 404 NotFound";
        }

        public override async Task<object> InternalServerError(Exception exception)
        {
            Response.Status = HttpStatus.InternalServerError;
            return exception;
        }
    }
}
