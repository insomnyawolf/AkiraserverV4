using System;

namespace AkiraserverV4.Http.Exceptions
{
    public class RoutingException : Exception
    {
        public RoutingException(string exception) : base(FormatException(exception))
        {
        }

        private static string FormatException(string ex)
        {
            return "The following endpoints cannot be mapped, their path should not be repeated:\n" + ex;
        }
    }

    public class MultipleMatchException : Exception
    {
        public MultipleMatchException(string args) : base($"Only one method can have the '{args}' attribute.")
        {
        }
    }
}