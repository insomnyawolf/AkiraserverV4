using SuperSimpleHttpListener.Http.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public class Headers
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public HttpVersion Version { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
    }
}
