using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using System.IO;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public abstract class ResponseResult
    {
        public ContentType ContentType { get; set; }
        public object Content { get; set; }

        public ResponseResult(object obj)
        {
            Content = obj;
        }

        public abstract string Serialize();
    }

    public abstract class BinaryResponseResult
    {
        public MemoryStream Content { get; set; }
        public abstract Task CustomResponse(BaseContext response);
    }
}