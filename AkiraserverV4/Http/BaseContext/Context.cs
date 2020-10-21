using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.BaseContext.Responses;
using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContext
{
    public abstract partial class Ctx : IDisposable
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public NetworkStream NetworkStream { get; private set; }

        private bool HeadersWritten;

        public Ctx() { }

        internal async Task WriteBodyAsync()
        {
            var data = Response.Body;
            if (data is null)
            {
                if (Response.Status == HttpStatus.Ok)
                {
                    Response.Status = HttpStatus.NoContent;
                }
            }
            else if (data is ResponseResult responseResult)
            {
                await SendResponseResultAsync(responseResult).ConfigureAwait(false);
            }
            else if (data is object)
            {
                await SendTextAsync(data).ConfigureAwait(false);
            }
        }

        public async Task WriteDataAsync(byte[] data)
        {
            await WriteDataAsync(new MemoryStream(data)).ConfigureAwait(false);
        }

        public async Task WriteDataAsync(Stream data)
        {
            await WriteHeadersAsync().ConfigureAwait(false);
            await data.CopyToAsync(NetworkStream).ConfigureAwait(false);
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
            Response.AddContentTypeHeader(data.ContentType);
            await SendTextAsync(data.Serialize()).ConfigureAwait(false);
        }

        internal async Task SendTextAsync(object input)
        {
#warning Moove to constants / enum

            Response.AddContentTypeHeader("text/plain");
            byte[] responseBytes = Encoding.UTF8.GetBytes(Convert.ToString(input));
            Response.AddContentLenghtHeader(responseBytes.Length);
            await WriteDataAsync(responseBytes).ConfigureAwait(false);
        }

        internal async Task SendRawAsync(object data)
        {
            using (Stream dataStream = data.ToStream())
            {
                Response.AddContentLenghtHeader(Convert.ToInt32(dataStream.Length));
#warning Moove to constants / enum
                Response.AddContentTypeHeader("application/octet-stream");

                await WriteDataAsync(dataStream).ConfigureAwait(false);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Request = null;
                    Response = null;
                    NetworkStream.Flush();
                    NetworkStream.Close();
                    // TODO: elimine el estado administrado (objetos administrados).
                }

                // TODO: libere los recursos no administrados (objetos no administrados) y reemplace el siguiente finalizador.
                // TODO: configure los campos grandes en nulos.

                disposedValue = true;
            }
        }

        // TODO: reemplace un finalizador solo si el anterior Dispose(bool disposing) tiene código para liberar los recursos no administrados.
        // ~Context()
        // {
        //   // No cambie este código. Coloque el código de limpieza en el anterior Dispose(colocación de bool).
        //   Dispose(false);
        // }

        // Este código se agrega para implementar correctamente el patrón descartable.
        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el anterior Dispose(colocación de bool).
            Dispose(true);
            // TODO: quite la marca de comentario de la siguiente línea si el finalizador se ha reemplazado antes.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}