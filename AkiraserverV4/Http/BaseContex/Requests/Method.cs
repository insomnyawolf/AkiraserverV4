namespace AkiraserverV4.Http.ContextFolder.RequestFolder
{
    public static class HttpMethodConvert
    {
        public static HttpMethod FromString(string raw)
        {
            return raw switch
            {
                "GET" => HttpMethod.Get,
                "HEAD" => HttpMethod.Head,
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                "CONNECT" => HttpMethod.Connect,
                "OPTIONS" => HttpMethod.Options,
                "TRACE" => HttpMethod.Trace,
                "PATCH" => HttpMethod.Patch,
                _ => HttpMethod.Other
            };
        }
    }

    public enum HttpMethod
    {
        Any,
        Get,
        Head,
        Post,
        Put,
        Delete,
        Connect,
        Options,
        Trace,
        Patch,
        Other
    }
}