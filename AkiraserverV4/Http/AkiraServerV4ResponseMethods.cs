using AkiraserverV4.Http.BaseContext;
using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.BaseContext.Responses;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static AkiraserverV4.Http.BaseContext.Ctx;
using static AkiraserverV4.Http.DelegateFactory;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private static async Task<object> InvokeHandlerAsync(BaseContext.BaseContext context, ExecutedCommand executedCommand, Exception exception = null)
        {
            if (context.NetworkStreamFailed)
            {
                return null;
            }

            dynamic data = await InvokeNamedParams(context, executedCommand, exception).ConfigureAwait(false);

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

        private static async Task<object> InvokeNamedParams(BaseContext.BaseContext context, ExecutedCommand executedCommand, Exception exception = null)
        {
#warning rework invoke with named params
            //executedCommand.MethodExecuted.HasProperty
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters = context.Request.UrlQuery;
            var parameters = await MapParameters(executedCommand.MethodInfo, context.Request).ConfigureAwait(false);

            return Invoke(methodExecuted: executedCommand.MethodExecuted, context: context, parameters: parameters);
        }

        private static object Invoke(object methodExecuted, BaseContext.BaseContext context, params object[] parameters)
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

        public static async Task<object[]> MapParameters(MethodInfo method, Request request)
        {
            ParameterInfo[] paramInfos = method.GetParameters().ToArray();
            string[] paramNames = paramInfos.Select(p => p.Name).ToArray();
            object[] parameters = new object[paramNames.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo currentParam = paramInfos[i];
                // If object type should try to get the value of the body (json/xml/form) else map query parameters into it

                if (currentParam.ParameterType.IsValueType
                    || currentParam.ParameterType.UnderlyingSystemType.IsValueType
                    || currentParam.ParameterType == typeof(string))
                {
                    parameters[i] = currentParam.ConvertValue(default);

                    if (currentParam.GetCustomAttribute<RequestDataBindingAttribute>() is RequestDataBindingAttribute test)
                    {
                        var item = request.UrlQuery.FormInput.SingleOrDefault(item => item.Name == currentParam.Name);
                        if (item != null)
                        {
                            int paramIndex = Array.IndexOf(paramNames, currentParam.Name);
                            if (paramIndex >= 0)
                            {
                                parameters[paramIndex] = paramInfos[paramIndex].ConvertValue(item.Value);
                            }
                        }
                    }
                }
                else if (request.Headers.ContainsKey(Header.ContentType))
                {
                    if (request.Headers[Header.ContentType].StartsWith(JsonDeserialize.ContentType))
                    {
                        parameters[i] = JsonDeserialize.DeSerialize(currentParam.ParameterType, request.ReadStringPayload());
                    }
                    // Map body Object Here
                }

            }

            return parameters;
        }
    }
}