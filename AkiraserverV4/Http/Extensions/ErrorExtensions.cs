using System;

namespace AkiraserverV4.Http.Extensions
{
    public static class ErrorExtensions
    {
        public static string ToErrorString(this string error, object context)
        {
            var now = DateTime.Now;
            var location = context.GetType().FullName;
            return $"{now}|{location}\n\t{error}";
        }
    }
}