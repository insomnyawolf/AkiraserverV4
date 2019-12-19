using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Extensions
{
    class NetworkStreamExtensions
    {
    }
    public static class TcpStreamExtension
    {
        public static async Task<int> ReadAsyncWithTimeout(this NetworkStream stream, byte[] buffer, int offset, int count, int TimeOut)
        {
            var ReciveCount = 0;
            var receiveTask = Task.Run(async () => { ReciveCount = await stream.ReadAsync(buffer, offset, count); });
            var isReceived = await Task.WhenAny(receiveTask, Task.Delay(TimeOut)) == receiveTask;
            // if (!isReceived) return -1;
            return ReciveCount;
        }
    }
}
