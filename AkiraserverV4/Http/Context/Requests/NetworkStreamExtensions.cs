using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context.Requests
{
    public static class TcpStreamExtension
    {
        public static async Task<int> ReadAsyncWithTimeout(this BufferedStream stream, byte[] buffer, int readLenght)
        {
            var cancelationToken = new CancellationTokenSource(stream.ReadTimeout);

            Task<int> result = stream.ReadAsync(buffer, 0, readLenght, cancelationToken.Token);

            if (result.IsCanceled)
            {
                return -1;
            }

            return await result;
        }
    }
}