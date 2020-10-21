using AkiraserverV4.Http.Helper;

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
}