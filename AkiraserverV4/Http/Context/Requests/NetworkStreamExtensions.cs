using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context.Requests
{
    public static class TcpStreamExtension
    {
        public static async Task<int> ReadAsyncWithTimeout(this BufferedStream stream, byte[] buffer)
        {
            var cancelationTokenSource = new CancellationTokenSource(stream.UnderlyingStream.ReadTimeout);

            var token = cancelationTokenSource.Token;

            Task<int> result = stream.ReadAsync(buffer, 0, buffer.Length, token);

            try
            {
                await result;
            }
            catch (OperationCanceledException Ex)
            {
                return -1;
            }

            return await result;
        }
    }
}