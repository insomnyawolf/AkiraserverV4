using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public abstract class ResponseResult
    {
        public abstract ContentType ContentType { get; set; }
        public abstract Task SerializeToNetworkStream(Response Response);
    }
}