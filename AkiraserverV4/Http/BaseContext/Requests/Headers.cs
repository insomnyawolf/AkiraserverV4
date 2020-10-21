using SuperSimpleHttpListener.Http.Helper;
using System.Collections.Generic;
using System.IO;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public class Headers
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion Version { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
    }

    public class RequestData
    {
        public Headers Headers { get; set; }
        public MemoryStream Body { get; set; }
    }
}
