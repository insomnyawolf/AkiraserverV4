using Extensions;
using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.SerializeHelpers;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context.Responses
{
    public partial class Response
    {
        internal async Task WriteBodyAsync(ExecutionStatus executionStatus)
        {
            if (executionStatus.ReturnType == ReturnType.Void)
            {
                if (HttpResponseHeaders.Status == HttpStatus.Ok)
                {
                    HttpResponseHeaders.Status = HttpStatus.NoContent;
                }
                await WriteHeaders();
            }

            if (executionStatus.Value is null)
            {
                await SendTextAsync("null");
            }
            else if (executionStatus.Value is ResponseResult responseResult)
            {
                await SendResponseResultAsync(responseResult);
            }
            else if (executionStatus.Value is MemoryStream stream)
            {
                await WriteDataAsync(stream);
            }
            else if (executionStatus.Value is string str)
            {
                await SendTextAsync(str);
            }
            else
            {
                await SendTextAsync(executionStatus.Value.ToString());
            }
        }

        /// <summary>
        /// Writes the current headers into the network stream if they were not written before
        /// </summary>
        /// <returns></returns>
        public async Task WriteHeaders()
        {
            if (!HeadersWritten)
            {
                var headersString = HttpResponseHeaders.Serialize();

                await StreamWriter.WriteAsync(headersString);

                await StreamWriter.FlushAsync();

                HeadersWritten = true;
            }
        }

        internal async Task SendResponseResultAsync<T>(T responseResult) where T : ResponseResult
        {
            AddContentType(responseResult.ContentType);
            await WriteHeaders();

            await responseResult.SerializeToNetworkStream(this);
        }

        internal async Task SendTextAsync(string input)
        {
            AddContentType(ContentType.HTML);
            AddContentLenght(input.Length);

            await WriteHeaders();
            await StreamWriter.WriteAsync(input);
            await StreamWriter.FlushAsync();
        }

        public async Task WriteDataAsync(Stream data)
        {
            AddContentType(ContentType.Binary);
            AddContentLenght(data.Length);
            await WriteHeaders();

            // is this really that good?

            data.Position = 0;
            await data.CopyToAsync(NetworkStream).ConfigureAwait(false);
            await NetworkStream.FlushAsync().ConfigureAwait(false);
        }
    }
}