using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.BaseContex
{
    internal static class ContextBuilder
    {
        public static async Task<BaseContext> CreateContext(Type target, NetworkStream networkStream, Request request, ServiceProvider serviceProvider)
        {
            Type BaseTypeOfContext = typeof(BaseContext);

            while (BaseTypeOfContext != typeof(BaseContext))
            {
                BaseTypeOfContext = BaseTypeOfContext.BaseType;
            }

            var Context = ActivatorUtilities.CreateInstance(serviceProvider, target);
            BaseTypeOfContext.GetProperty("NetworkStream").SetValue(obj: Context, value: networkStream);
            BaseTypeOfContext.GetProperty("Request").SetValue(obj: Context, value: request);
            BaseTypeOfContext.GetProperty("Response").SetValue(obj: Context, value: new Response());
            return (BaseContext)Context;
        }
    }
}