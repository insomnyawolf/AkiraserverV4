using AkiraserverV4.Http.Helper;
using System.Collections.Generic;
using System.IO;

namespace AkiraserverV4.Http.Context.Requests
{
    public class Header
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion Version { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
    }

    public class RequestData
    {
        public Header Headers { get; set; }
        public MemoryStream Body { get; set; }
    }
}
