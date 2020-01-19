using System;

namespace SuperSimpleHttpListener.Http.Helper
{
    public static class HttpVersionConvert
    {
        public static HttpVersion FromString(string raw)
        {
            return raw switch
            {
                "HTTP/1.0" => HttpVersion.HTTP10,
                "HTTP/1.1" => HttpVersion.HTTP11,
                "HTTP/2.0" => HttpVersion.HTTP20,
                _ => throw new NotImplementedException($"The version '{raw}' is not implemented.")
            };
        }

        public static string ToVersionString(this HttpVersion version)
        {
            return version switch
            {
                HttpVersion.HTTP10 => "HTTP/1.0",
                HttpVersion.HTTP11 => "HTTP/1.1",
                HttpVersion.HTTP20 => "HTTP/2.0",
                _ => throw new NotImplementedException($"The version '{version}' is not implemented.")
            };
        }
    }

    public enum HttpVersion
    {
        HTTP10,
        HTTP11,
        HTTP20
    }
}