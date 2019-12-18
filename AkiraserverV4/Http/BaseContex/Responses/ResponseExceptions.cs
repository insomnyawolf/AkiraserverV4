using System;

namespace AkiraserverV4.Http.ContextFolder.ResponseFolder
{
    public class ContentLenghtHeaderNotFoundException : Exception
    {
        public ContentLenghtHeaderNotFoundException(string message) : base(message)
        {
        }

        public ContentLenghtHeaderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ContentLenghtHeaderNotFoundException()
        {
        }
    }
}