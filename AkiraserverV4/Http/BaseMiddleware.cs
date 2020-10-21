using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using System;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public class BaseMiddleware
    {
        public BaseContext Context { get; set; }

        public virtual async Task<object> ActionExecuting(ExecutedCommand executedCommand)
        {
            return await Context.InvokeHandlerAsync(executedCommand).ConfigureAwait(false);
        }

        public virtual async Task<object> BadRequest(Exception exception)
        {
            Context.Response.Status = HttpStatus.BadRequest;
            return exception;
        }

        public virtual async Task<object> NotFound(Request request)
        {
            Context.Response.Status = HttpStatus.NotFound;
            return "404 NotFound";
        }

        public virtual async Task<object> InternalServerError(Exception exception)
        {
            Context.Response.Status = HttpStatus.InternalServerError;
            return exception;
        }
    }
}
