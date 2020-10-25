using AkiraserverV4.Http;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SampleServer.Middlewares
{
    public class Middleware : BaseMiddleware
    {
        private static readonly ILogger<Middleware> Logger = Program.ServiceProvider.GetRequiredService<ILogger<Middleware>>();

        public override async Task<ExecutionStatus> ActionExecuting(ExecutedCommand executedCommand)
        {
            return await InvokeNamedParams(Context, executedCommand).ConfigureAwait(false);
        }

        public override async Task<object> BadRequest(Exception exception)
        {
            Context.Response.Status = HttpStatus.BadRequest;
            return exception;
        }

        public override async Task<object> NotFound(Request request)
        {
            Context.Response.Status = HttpStatus.NotFound;
            return $"404 NotFound => {request.Header.Method} {request.Header.Path}";
        }

        public override async Task<object> InternalServerError(Exception exception)
        {
            Context.Response.Status = HttpStatus.InternalServerError;
            return exception;
        }
    }
}
