using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;

namespace AkiraserverV4.Http.Context
{
    internal static class ContextBuilder
    {
        private static readonly Type BaseTypeOfContext = typeof(BaseContext);
        public static BaseMiddleware CreateContext(ExecutedCommand target, Type Middleware, NetworkStream networkStream, Request request, Response response, IServiceProvider serviceProvider)
        {
            object Context;

            Context = ActivatorUtilities.CreateInstance(serviceProvider, target.ReflectedDelegate.DeclaringType);

            BaseTypeOfContext.GetProperty(nameof(BaseContext.NetworkStream)).SetValue(obj: Context, value: networkStream);
            BaseTypeOfContext.GetProperty(nameof(BaseContext.Request)).SetValue(obj: Context, value: request);
            BaseTypeOfContext.GetProperty(nameof(BaseContext.Response)).SetValue(obj: Context, value: response);

            var middleware = ActivatorUtilities.CreateInstance(serviceProvider, Middleware);
            Middleware.GetProperty(nameof(BaseMiddleware.Context)).SetValue(obj: middleware, value: Context);

            return (BaseMiddleware)middleware;
        }
    }
}