using System;

namespace SuperSimpleHttpListener.Http.Request
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
                _ => throw new NotImplementedException($"Method '{raw}' is not implemented.")
            };
        }
    }

    public enum HttpMethod
    {
        Get,
        Head,
        Post,
        Put,
        Delete,
        Connect,
        Options,
        Trace,
        Patch
    }
}