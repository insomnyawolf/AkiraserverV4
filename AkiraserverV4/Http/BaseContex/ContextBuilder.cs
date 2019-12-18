using AkiraserverV4.Http.ContextFolder.RequestFolder;
using AkiraserverV4.Http.ContextFolder.ResponseFolder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.ContextFolder
{
    internal static class ContextBuilder
    {
        public static async Task<Context> CreateContext(Type target, NetworkStream networkStream, Request request, ServiceProvider serviceProvider)
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