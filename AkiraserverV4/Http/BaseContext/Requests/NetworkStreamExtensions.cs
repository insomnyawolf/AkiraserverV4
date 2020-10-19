using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContext.Requests
{
    public static class TcpStreamExtension
    {
        //        public static async Task<int> ReadAsyncWithTimeout(this NetworkStream stream, byte[] buffer, int offset, int count)
        //        {
        //            try
        //            {
        //                var ReciveCount = 0;
        //                Task dataTask = stream.ReadAsync(buffer, offset, count);
        //                Task cancelTask = Task.Delay(stream.ReadTimeout);
        //                var isReceived = await Task.WhenAny(dataTask, cancelTask) == dataTask;
        //                if (isReceived)
        //                    return ReciveCount;
        //            }
        //            catch (IOException)
        //            {
        //#warning proper error handling
        //            }
        //            return -1;
        //        }

        //public static async Task<int> ReadPacketAsync(this NetworkStream networkStream, byte[] buffer, int packetSize)
        //{
        //    return await networkStream.ReadAsyncWithTimeout(buffer: buffer, offset: 0, count: packetSize, TimeOut: networkStream.ReadTimeout).ConfigureAwait(false);
        //}
        public static async Task<int> ReadAsyncWithTimeout(this NetworkStream stream, byte[] buffer, int timeout)
        {
            Task<int> result = stream.ReadAsync(buffer, 0, buffer.Length);

            await Task.WhenAny(result, Task.Delay(timeout));

            if (result.IsCompleted)
            {
                return await result;
            }

            return -1;
        }
    }


}