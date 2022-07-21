using AkiraserverV4.Http.Context.Requests;
using System;

namespace AkiraserverV4.Http.Context
{
    [Controller]
    public partial class BaseContext
    {
        [BadRequestHandler]
        public virtual Exception BadRequest(Exception exception)
        {
            return exception;
        }

        [NotFoundHandler]
        public virtual string NotFound(Request request)
        {
            return "404 NotFound";
        }

        [InternalServerErrorHandler]
        public virtual Exception InternalServerError(Exception exception)
        {
            return exception;
        }
    }
}