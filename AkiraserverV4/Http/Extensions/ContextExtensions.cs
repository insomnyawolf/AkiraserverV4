using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Extensions
{
    internal static class ContextExtensions
    {
        internal static async Task SendText(this Context context, object input)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(Convert.ToString(input));
            if (!context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.AddContentLenghtHeader(responseBytes.Length);
            }
            await context.WriteDataAsync(responseBytes);
        }

        internal static async Task SendRaw(this Context context, object data)
        {
            using (Stream dataStream = data.ToStream())
            {
                if (!context.Response.Headers.ContainsKey("Content-Length"))
                {
                    context.Response.AddContentLenghtHeader(Convert.ToInt32(dataStream.Length));
                }
                await context.WriteDataAsync(dataStream);
            }
        }

        internal static async Task SendJson<T>(this Context context, T data) where T : JsonResult
        {
            await context.SendText(data.SerializedJson);
        }
    }
}