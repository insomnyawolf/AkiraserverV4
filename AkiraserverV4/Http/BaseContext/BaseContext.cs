﻿using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context
{
    public partial class BaseContext
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public NetworkStream NetworkStream { get; private set; }

        private bool HeadersWritten;

        public BaseContext() { }

        internal async Task WriteBodyAsync(bool isReturnTypeVoid, object bodyContent)
        {
            if (isReturnTypeVoid)
            {
                if (Response.Status == HttpStatus.Ok)
                {
                    Response.Status = HttpStatus.NoContent;
                }
                await WriteHeadersAsync().ConfigureAwait(false);
            }

            if (bodyContent is ResponseResult responseResult)
            {
                await SendResponseResultAsync(responseResult).ConfigureAwait(false);
            }
            else if (bodyContent is BinaryResponseResult binaryResponse)
            {
                await binaryResponse.CustomResponse(this).ConfigureAwait(false);
                await WriteDataAsync(binaryResponse.Content).ConfigureAwait(false);

            }
            else if (bodyContent is MemoryStream stream)
            {
                await WriteDataAsync(stream).ConfigureAwait(false);
            }
            else if (bodyContent is null)
            {
                bodyContent = "null";
            }
            await SendTextAsync(bodyContent).ConfigureAwait(false);
        }

        public async Task WriteDataAsync(byte[] data)
        {
            await WriteHeadersAsync().ConfigureAwait(false);

            await NetworkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await NetworkStream.FlushAsync().ConfigureAwait(false);
        }

        public async Task WriteDataAsync(MemoryStream data)
        {
            await WriteHeadersAsync().ConfigureAwait(false);

            // is this really that good?

            data.Position = 0;
            await data.CopyToAsync(NetworkStream).ConfigureAwait(false);
            await NetworkStream.FlushAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the current headers into the network stream if they were not written before
        /// </summary>
        /// <returns></returns>
        public async Task WriteHeadersAsync()
        {
            if (!HeadersWritten)
            {
                byte[] headers = Response.ProcessHeaders().ToByteArray();

                await NetworkStream.WriteAsync(headers, 0, headers.Length).ConfigureAwait(false);

                HeadersWritten = true;
            }
        }

        internal async Task SendResponseResultAsync<T>(T data) where T : ResponseResult
        {
            Response.AddContentType(data.ContentType);
            await SendTextAsync(data.Serialize()).ConfigureAwait(false);
        }

        internal async Task SendTextAsync(object input)
        {
#warning Moove to constants / enum

            Response.AddContentType(ContentType.PlainText);
            byte[] responseBytes = Encoding.UTF8.GetBytes(Convert.ToString(input));
            Response.AddContentLenght(responseBytes.Length);
            await WriteDataAsync(responseBytes).ConfigureAwait(false);
        }

        internal async Task SendRawAsync(object data)
        {
            using (MemoryStream dataStream = data.ToStream())
            {
                Response.AddContentLenght(Convert.ToInt32(dataStream.Length));
#warning Moove to constants / enum
                Response.AddContentType(ContentType.Binary);

                await WriteDataAsync(dataStream).ConfigureAwait(false);
            }
        }
    }
}