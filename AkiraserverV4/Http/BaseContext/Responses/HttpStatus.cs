using System.Collections.Generic;

namespace AkiraserverV4.Http.Context.Responses
{
    public static class HttpStatusConvert
    {
        public static readonly Dictionary<HttpStatus, string> RespStatus = new Dictionary<HttpStatus, string>()
        {
            //"100"  ; Section 10.1.1: Continue
            //"101"  ; Section 10.1.2: Switching Protocols

            // Ok

            { HttpStatus.Ok, "200 Ok" },
            { HttpStatus.Created, "201 Created" },
            { HttpStatus.Accepted, "202 Accepted" },
            { HttpStatus.NonAuthoritative, "203 Non-Authoritative" },
            { HttpStatus.NoContent, "204 No Content" },
            { HttpStatus.ResetContent, "205 Reset Content" },
            { HttpStatus.PartialContent, "204 Partial Content" },

            // Redirect

            { HttpStatus.MultipleChoices, "300 Multiple Choices" },
            { HttpStatus.MovedPermanently, "301 Moved Permanently" },
            { HttpStatus.Redirection, "302 Redirection" },
            { HttpStatus.SeeOther, "303 See Other" },
            { HttpStatus.NotModified, "304 Not Modified" },
            { HttpStatus.UseProxy, "305 Use Proxy" },
            { HttpStatus.TemporaryRedirect, "307 Temporary Redirect" },

            // Client Error

            { HttpStatus.BadRequest, "400 Bad Request" },
            { HttpStatus.Unauthorized, "401 Unauthorized" },
            { HttpStatus.PaymentRequired, "402 Payment Required" },
            { HttpStatus.Forbidden, "403 Forbidden" },
            { HttpStatus.NotFound, "404 Not Found" },
            { HttpStatus.MethodNotAllowed, "405 Method Not Allowed" },
            { HttpStatus.NotAcceptable, "406 Not Acceptable" },
            { HttpStatus.ProxyAuthenticationRequired, "407 Proxy Authentication Required" },
            { HttpStatus.RequestTimeOut, "408 Request Time-out" },
            { HttpStatus.Conflict, "409 Conflict" },
            { HttpStatus.Gone, "410 Gone" },
            { HttpStatus.LengthRequired, "411 Length Required" },
            { HttpStatus.PreconditionFailed, "412 Precondition Failed" },
            { HttpStatus.RequestEntityTooLarge, "413 Request Entity Too Large" },
            { HttpStatus.RequestURITooLarge, "414 Request-URI Too Large" },
            { HttpStatus.UnsupportedMediaType, "415 Unsupported Media Type" },
            { HttpStatus.RequestedRangeNotSatisfiable, "416 Requested range not satisfiable" },
            { HttpStatus.ExpectationFailed, "417 Expectation Failed" },
            { HttpStatus.ImTeapot, "418 I'm a teapot" },

            // Server Error

            { HttpStatus.InternalServerError, "500 Internal Server Error" },
            { HttpStatus.NotImplemented, "501 Not Implemented" },
            { HttpStatus.BadGateway, "502 Bad Gateway" },
            { HttpStatus.ServiceUnavailable, "503 Service Unavailable" },
            { HttpStatus.GatewayTimeOut, "504 Gateway Time-out" },
            { HttpStatus.HTTPVersionNotSupported, "505 HTTP Version not supported" },
        };

        public static string ToStatusString(this HttpStatus status)
        {
            if (!RespStatus.ContainsKey(status))
            {
                throw new KeyNotFoundException($"Can't found the status '{status}' in the available status string.");
            }
            return RespStatus[status];
        }
    }

    public enum HttpStatus
    {
        // Ok

        Ok = 200,
        Created = 201,
        Accepted = 202,
        NonAuthoritative = 203,
        NoContent = 204,
        ResetContent = 205,
        PartialContent = 206,

        // Redirect

        MultipleChoices = 300,
        MovedPermanently = 301,
        Redirection = 302,
        SeeOther = 303,
        NotModified = 304,
        UseProxy = 305,
        TemporaryRedirect = 307,

        // Client Error

        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,
        RequestTimeOut = 408,
        Conflict = 409,
        Gone = 410,
        LengthRequired = 411,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        RequestURITooLarge = 414,
        UnsupportedMediaType = 415,
        RequestedRangeNotSatisfiable = 416,
        ExpectationFailed = 417,
        ImTeapot = 418,

        // Server Error

        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeOut = 504,
        HTTPVersionNotSupported = 505,
    }
}