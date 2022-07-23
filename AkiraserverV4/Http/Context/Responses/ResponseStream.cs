using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.SerializeHelpers;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Extensions.Reflection;

namespace AkiraserverV4.Http.Context.Responses
{
    public partial class Response
    {
        internal async ValueTask WriteBodyAsync(ExecutionStatus executionStatus)
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

            await StreamWriter.FlushAsync();
            await NetworkStream.FlushAsync();
        }

        /// <summary>
        /// Writes the current headers into the network stream if they were not written before
        /// </summary>
        /// <returns></returns>
        public async ValueTask WriteHeaders()
        {
            if (!HeadersWritten)
            {
                var headersString = HttpResponseHeaders.Serialize();

                await StreamWriter.WriteAsync(headersString);

                HeadersWritten = true;
            }
        }

        internal async ValueTask SendResponseResultAsync<T>(T responseResult) where T : ResponseResult
        {
            AddContentType(responseResult.ContentType);

            await WriteHeaders();

            await responseResult.SerializeToNetworkStream(this);
        }

        internal async ValueTask SendTextAsync(string input)
        {
            AddContentType(ContentType.HTML);

            AddContentLenght(input.Length);

            await WriteHeaders();

            await StreamWriter.WriteAsync(input);
        }

        public async ValueTask WriteDataAsync(Stream data)
        {
            AddContentType(ContentType.Binary);

            AddContentLenght(data.Length);
            
            await WriteHeaders();

            // is this really that good?
            data.Position = 0;
            await data.CopyToAsync(NetworkStream);
        }
    }
}