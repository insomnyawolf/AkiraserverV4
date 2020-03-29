using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Responses;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AkiraserverV4.Http.DelegateFactory;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private async Task InvokeHandlerAsync(Context context, ExecutedCommand executedCommand, Exception exception = null)
        {
            if (context.NetworkStreamFailed)
            {
                return;
            }

            dynamic data = InvokeNamedParams(context, executedCommand, exception);

            if (data is Task)
            {
                await data;

                if (executedCommand.ReturnIsGenericType)
                {
                    data = data.Result;
                }
                else
                {
                    data = null;
                }
            }

            await ProcessResponse(context, data);
        }

        private object InvokeNamedParams(Context context, ExecutedCommand executedCommand, Exception exception = null)
        {
            if (exception != null)
            {
                Dictionary<string, object> exceptions = new Dictionary<string, object>
                {
                    { "exception", exception }
                };

                return Invoke(methodExecuted: executedCommand.MethodExecuted, context: context, parameters: exceptions);
            }

#warning rework invoke with named params
            //executedCommand.MethodExecuted.HasProperty
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters = context.Request.UrlQuery;

            return Invoke(methodExecuted: executedCommand.MethodExecuted, context: context);
        }

        private object Invoke(object methodExecuted, Context context, params object[] parameters)
        {
            try
            {
                if (methodExecuted is ReflectedDelegate reflectedDelegate)
                {
                    return reflectedDelegate(context, parameters);
                }
                else if (methodExecuted is ReflectedVoidDelegate action)
                {
                    action(context, parameters);
                }
            }
            catch (Exception)
            {
                // Exception Handling Middleware
            }
            return null;
        }

        private async Task ProcessResponse(Context context, object data)
        {
            if (data is null)
            {
                if (context.Response.Status == HttpStatus.Ok)
                {
                    context.Response.Status = HttpStatus.NoContent;
                }
            }
            else if (data is JsonResult jsonSerializable)
            {
                await context.SendJsonAsync(jsonSerializable).ConfigureAwait(false);
            }
            else if (data is object)
            {
                await context.SendTextAsync(data).ConfigureAwait(false);
            }
        }
    }
}