using AkiraserverV4.Http.ContextFolder.RequestFolder;
using AkiraserverV4.Http.ContextFolder.ResponseFolder;
using AkiraserverV4.Http.Helper;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.ContextFolder
{
    public abstract partial class Context : IDisposable
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public NetworkStream NetworkStream { get; private set; }

        private bool HeadersWritten = false;

        public async Task WriteData(byte[] data)
        {
            await WriteHeaders();
            await NetworkStream.WriteAsync(data, 0, data.Length);
        }

        public async Task WriteHeaders()
        {
            if (!HeadersWritten)
            {
                byte[] headers = Response.ProcessHeaders().ToByteArray();
                await NetworkStream.WriteAsync(headers, 0, headers.Length);
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