using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using System.Net.Sockets;

namespace AkiraserverV4.Http.Context
{
    public partial class BaseContext
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public NetworkStream NetworkStream => Response.NetworkStream;
        public BaseContext() { }
    }
}