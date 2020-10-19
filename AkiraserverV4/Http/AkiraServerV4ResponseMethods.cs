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
        private static async Task<object> InvokeHandlerAsync(BaseContext.Ctx context, ExecutedCommand executedCommand, Exception exception = null)
        {
            if (context.NetworkStreamFailed)
            {
                return null;
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

            return data;
        }

        private static object InvokeNamedParams(BaseContext.Ctx context, ExecutedCommand executedCommand, Exception exception = null)
        {
#warning rework invoke with named params
            //executedCommand.MethodExecuted.HasProperty
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters = context.Request.UrlQuery;

            return Invoke(methodExecuted: executedCommand.MethodExecuted, context: context);
        }

        private static object Invoke(object methodExecuted, BaseContext.Ctx context, params object[] parameters)
        {
            if (methodExecuted is ReflectedDelegate reflectedDelegate)
            {
                return reflectedDelegate(context, parameters);
            }
            else if (methodExecuted is ReflectedVoidDelegate action)
            {
                action(context, parameters);
            }
            return null;
        }
    }
}