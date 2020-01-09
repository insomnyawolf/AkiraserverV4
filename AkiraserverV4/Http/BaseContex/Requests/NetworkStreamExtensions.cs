#define DontReturnEOF

using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContex.Requests
{
    public static class TcpStreamExtension
    {
        public static async Task<int> ReadAsyncWithTimeout(this NetworkStream stream, byte[] buffer, int offset, int count, int TimeOut)
        {
            var ReciveCount = 0;
            var receiveTask = Task.Run(async () => ReciveCount = await stream.ReadAsync(buffer, offset, count));
#if ReturnEOF
            var isReceived = await Task.WhenAny(receiveTask, Task.Delay(TimeOut)) == receiveTask;
            if (!isReceived) return -1;
#else
            _ = await Task.WhenAny(receiveTask, Task.Delay(TimeOut)) == receiveTask;
#endif
            return ReciveCount;
        }
    }
}