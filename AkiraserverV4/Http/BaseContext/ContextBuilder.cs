using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.BaseContext.Responses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;

namespace AkiraserverV4.Http.BaseContext
{
    internal static class ContextBuilder
    {
        private static readonly Type BaseTypeOfContext = typeof(Ctx);
        public static BaseContext CreateContext(Type target, NetworkStream networkStream, Request request, IServiceProvider serviceProvider)
        {
            var Context = ActivatorUtilities.CreateInstance(serviceProvider, target);
            BaseTypeOfContext.GetProperty("NetworkStream").SetValue(obj: Context, value: networkStream);
            BaseTypeOfContext.GetProperty("Request").SetValue(obj: Context, value: request);
            BaseTypeOfContext.GetProperty("Response").SetValue(obj: Context, value: new Response());
            return (BaseContext)Context;
        }
    }
}