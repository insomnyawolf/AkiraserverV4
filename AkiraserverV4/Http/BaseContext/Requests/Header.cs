using AkiraserverV4.Http.Helper;
using System.Collections.Generic;

namespace AkiraserverV4.Http.Context.Requests
{
    public class RequestHeaders : Dictionary<string, string>
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion Version { get; set; }
    }
}
