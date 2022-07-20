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
        public virtual Exception BadRequest(Exception exception)
        {
            Response.HttpResponseHeaders.Status = HttpStatus.BadRequest;
            return exception;
        }

        [NotFoundHandler]
        public virtual string NotFound(Request request)
        {
            Response.HttpResponseHeaders.Status = HttpStatus.NotFound;
            return "404 NotFound";
        }

        [InternalServerErrorHandler]
        public virtual Exception InternalServerError(Exception exception)
        {
            Response.HttpResponseHeaders.Status = HttpStatus.InternalServerError;
            return exception;
        }
    }
}