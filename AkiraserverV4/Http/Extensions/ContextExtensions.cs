using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Extensions
{
    internal static class ContextExtensions
    {
        internal static async Task SendText(this Context context, object text)
        {
#warning Optimize Already String Inputs

            byte[] responseBytes = Encoding.UTF8.GetBytes(text.ToString());
            if (!context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.AddContentLenghtHeader(responseBytes.Length);
            }
            await context.WriteDataAsync(responseBytes);
        }

        internal static async Task SendRaw(this Context context, object data)
        {
            throw new NotImplementedException();
        }

        internal static async Task SendObject(this Context context, object data)
        {
            throw new NotImplementedException();
        }

        internal static async Task SendJson<T>(this Context context, T data) where T : JsonResult
        {
            await context.SendText(data.SerializedJson);
        }
    }
}