using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.BaseContex.Requests
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
