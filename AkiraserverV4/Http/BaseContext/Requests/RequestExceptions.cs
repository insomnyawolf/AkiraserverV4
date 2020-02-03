using System;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message = null) : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}