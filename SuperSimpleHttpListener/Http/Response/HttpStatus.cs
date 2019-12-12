using System.Collections.Generic;
using System.Collections.Immutable;

namespace SuperSimpleHttpListener.Http.Response
{
    public static class HttpStatusConvert
    {
        public static readonly ImmutableDictionary<HttpStatus, string> RespStatus = new Dictionary<HttpStatus, string>()
        {
            // Ok

            { HttpStatus.Ok, "200 Ok" },
            { HttpStatus.Created, "201 Created" },
            { HttpStatus.Accepted, "202 Accepted" },
            { HttpStatus.NonAuthoritative, "203 Non-Authoritative" },
            { HttpStatus.NoContent, "204 No Content" },

            // Redirect

            { HttpStatus.MultipleChoices, "300 Multiple Choices" },
            { HttpStatus.MovedPermanently, "301 Moved Permanently" },
            { HttpStatus.Redirection, "302 Redirection" },
            { HttpStatus.SeeOther, "303 See Other" },
            { HttpStatus.NotModified, "304 Not Modified" },

            // Client Error

            { HttpStatus.BadRequest, "400 Bad Request" },
            { HttpStatus.Unauthorized, "401 Unauthorized" },
            { HttpStatus.PaymentRequired, "402 Payment Required" },
            { HttpStatus.Forbidden, "403 Forbidden" },
            { HttpStatus.NotFound, "404 Not Found" },

            // Server Error

            { HttpStatus.InternalServerError, "500 Internal Server Error" },
            { HttpStatus.NotImplemented, "501 Not Implemented" },
            { HttpStatus.BadGateway, "502 Bad Gateway" },
            { HttpStatus.ServiceUnavailable, "503 Service Unavailable" },
        }.ToImmutableDictionary();

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

        // Redirect

        MultipleChoices = 300,
        MovedPermanently = 301,
        Redirection = 302,
        SeeOther = 303,
        NotModified = 304,

        // Client Error

        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,

        // Server Error

        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
    }
}