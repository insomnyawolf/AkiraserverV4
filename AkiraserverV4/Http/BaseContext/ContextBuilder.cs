using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.BaseContext.Responses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;

namespace AkiraserverV4.Http.BaseContext
{
    internal static class ContextBuilder
    {
        public static Context CreateContext(Type target, NetworkStream networkStream, Request request, IServiceProvider serviceProvider)
        {
            Type BaseTypeOfContext = typeof(Context);

            while (BaseTypeOfContext != typeof(Context))
            {
                BaseTypeOfContext = BaseTypeOfContext.BaseType;
            }

            var Context = ActivatorUtilities.CreateInstance(serviceProvider, target);
            BaseTypeOfContext.GetProperty("NetworkStream").SetValue(obj: Context, value: networkStream);
            BaseTypeOfContext.GetProperty("Request").SetValue(obj: Context, value: request);
            BaseTypeOfContext.GetProperty("Response").SetValue(obj: Context, value: new Response());
            return (Context)Context;
        }
    }
}