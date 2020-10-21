using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public static class TcpStreamExtension
    {
        public static async Task<int> ReadAsyncWithTimeout(this NetworkStream stream, byte[] buffer, int readLenght, int timeout)
        {
            Task<int> result = stream.ReadAsync(buffer, 0, readLenght);

            await Task.WhenAny(result, Task.Delay(timeout));

            if (result.IsCompleted)
            {
                return await result;
            }

            return -1;
        }
    }


}