using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using System;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public class BaseMiddleware : IDisposable
    {
        public BaseContext Context { get; set; }

        public virtual async Task<object> ActionExecuting(ExecutedCommand executedCommand)
        {
            return await Context.InvokeHandlerAsync(executedCommand).ConfigureAwait(false);
        }

        public virtual async Task<object> BadRequest(Exception exception)
        {
            Context.Response.Status = HttpStatus.BadRequest;
            return exception;
        }

        public virtual async Task<object> NotFound(Request request)
        {
            Context.Response.Status = HttpStatus.NotFound;
            return "404 NotFound";
        }

        public virtual async Task<object> InternalServerError(Exception exception)
        {
            Context.Response.Status = HttpStatus.InternalServerError;
            return exception;
        }

        #region IDisposable Support

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~BaseMiddleware()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
