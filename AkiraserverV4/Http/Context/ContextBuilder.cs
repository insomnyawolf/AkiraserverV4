using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace AkiraserverV4.Http.Context
{
    internal static class ContextBuilder
    {
        private static readonly Type BaseTypeOfContext = typeof(BaseContext);
        private static PropertyInfo BaseContextRequestProperty = BaseTypeOfContext.GetProperty(nameof(BaseContext.Request));
        private static PropertyInfo BaseContextResponseProperty = BaseTypeOfContext.GetProperty(nameof(BaseContext.Response));
        public static BaseContext CreateContext(ExecutedCommand executedCommand, Request request, Response response, IServiceProvider serviceProvider)
        {
            if (executedCommand.ReflectedDelegate.IsStatic)
            {
                return null;
            }

            var Context = ActivatorUtilities.CreateInstance(serviceProvider, executedCommand.ReflectedDelegate.DeclaringType);
            BaseContextRequestProperty.SetValue(obj: Context, value: request);
            BaseContextResponseProperty.SetValue(obj: Context, value: response);

            return (BaseContext)Context;
        }
    }
}