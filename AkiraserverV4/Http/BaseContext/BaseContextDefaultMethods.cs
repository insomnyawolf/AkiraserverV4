using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using System;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context
{
    [Controller]
    public partial class BaseContext
    {
        [BadRequestHandler]
        public virtual async Task<Exception> BadRequest(Exception exception)
        {
            Response.Status = HttpStatus.BadRequest;
            return exception;
        }

        [NotFoundHandler]
        public virtual async Task<string> NotFound(Request request)
        {
            Response.Status = HttpStatus.NotFound;
            return "404 NotFound";
        }

        [InternalServerErrorHandler]
        public virtual async Task<Exception> InternalServerError(Exception exception)
        {
            Response.Status = HttpStatus.InternalServerError;
            return exception;
        }
    }
}