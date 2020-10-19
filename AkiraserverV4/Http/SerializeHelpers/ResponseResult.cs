using System;
using System.Text.Json;

namespace AkiraserverV4.Http.SerializeHelpers
{
    public abstract class ResponseResult
    {
        public object Content { get; set; }

        public ResponseResult(object obj)
        {
            Content = obj;
        }

        public abstract string Serialize();
    }
}