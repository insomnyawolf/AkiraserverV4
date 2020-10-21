using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.BaseContext.Responses;
using System;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContext
{
    public class BaseContext : Ctx
    {
        public virtual async Task<object> BadRequest(Exception exception)
        {
            Response.Status = HttpStatus.BadRequest;
            return exception;
        }

        public virtual async Task<object> NotFound(Request request)
        {
            Response.Status = HttpStatus.NotFound;
            return "404 NotFound";
        }

        public virtual async Task<object> InternalServerError(Exception exception)
        {
            Response.Status = HttpStatus.InternalServerError;
            return exception;
        }
    }
}
