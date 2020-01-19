using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContex
{
    public abstract partial class Context : IDisposable
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public NetworkStream NetworkStream { get; private set; }

        private bool HeadersWritten = false;

        public async Task WriteDataAsync(byte[] data)
        {
            await WriteHeadersAsync().ConfigureAwait(false);
            await NetworkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
        }

        public async Task WriteDataAsync(List<byte> data)
        {
            await WriteHeadersAsync().ConfigureAwait(false);

            for (int position = 0; position < data.Count; position++)
            {
                NetworkStream.WriteByte(data[position]);
            }
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