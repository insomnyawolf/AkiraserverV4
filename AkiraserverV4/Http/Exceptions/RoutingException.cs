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

    public class NoDefaultEndpointException : Exception
    {
        public NoDefaultEndpointException() : base("Can not find the default routing fallback.")
        {
        }
    }

    public class MultipleDefaultEndpointException : Exception
    {
        public MultipleDefaultEndpointException() : base("Multiple default fallbacks found.")
        {
        }
    }
}