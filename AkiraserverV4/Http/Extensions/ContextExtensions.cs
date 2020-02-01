using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Extensions
{
    internal static class ContextExtensions
    {
        internal static async Task SendTextAsync(this Context context, object input)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(Convert.ToString(input));
            if (!context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.AddContentLenghtHeader(responseBytes.Length);
            }
            await context.WriteDataAsync(responseBytes).ConfigureAwait(false);
        }

        internal static async Task SendRawAsync(this Context context, object data)
        {
            using (Stream dataStream = data.ToStream())
            {
                if (!context.Response.Headers.ContainsKey("Content-Length"))
                {
                    context.Response.AddContentLenghtHeader(Convert.ToInt32(dataStream.Length));
                }
                await context.WriteDataAsync(dataStream).ConfigureAwait(false);
            }
        }

        internal static async Task SendJsonAsync<T>(this Context context, T data) where T : JsonResult
        {
            await context.SendTextAsync(data.SerializedJson).ConfigureAwait(false);
        }
    }
}