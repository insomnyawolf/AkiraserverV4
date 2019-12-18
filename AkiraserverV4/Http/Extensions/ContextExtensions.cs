using AkiraserverV4.Http.ContextFolder;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Extensions
{
    internal static class ContextExtensions
    {
        internal static async Task SendText(this Context context, string text)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(text);
            if (!context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.AddContentLenghtHeader(responseBytes.Length);
            }
            await context.WriteData(responseBytes);
        }

        internal static async Task SendObject(this Context context, object data)
        {
            await context.SendText(JsonSerializer.Serialize(data));
        }
    }
}