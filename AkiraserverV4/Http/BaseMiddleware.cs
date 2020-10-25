using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Extensions;

namespace AkiraserverV4.Http
{
    public class BaseMiddleware
    {
        public BaseContext Context { get; set; }

        public virtual async Task<ExecutionStatus> ActionExecuting(ExecutedCommand executedCommand)
        {
            return await InvokeNamedParams(Context, executedCommand).ConfigureAwait(false);
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

        public static async Task<ExecutionStatus> InvokeNamedParams(BaseContext context, ExecutedCommand executedCommand)
        {
            var parameters = await MapParameters(executedCommand.ParameterInfo, context.Request).ConfigureAwait(false);

            return await Invoke(executedCommand: executedCommand, context: context, parameters: parameters).ConfigureAwait(false);
        }

        public static async Task<object[]> MapParameters(ParameterInfo[] paramInfos, Request request)
        {
#warning rework invoke with named params
            string[] paramNames = paramInfos.Select(p => p.Name).ToArray();
            object[] parameters = new object[paramNames.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo currentParam = paramInfos[i];
                // If object type should try to get the value of the body (json/xml/form) else map query parameters into it

                if (currentParam.ParameterType.IsValueType
                 || currentParam.ParameterType.UnderlyingSystemType.IsValueType
                 || currentParam.ParameterType == typeof(DateTime)
                 || currentParam.ParameterType == typeof(TimeSpan)
                 || currentParam.ParameterType == typeof(DateTimeOffset)
                 || currentParam.ParameterType == typeof(DateTime?)
                 || currentParam.ParameterType == typeof(TimeSpan?)
                 || currentParam.ParameterType == typeof(DateTimeOffset?)
                 || currentParam.ParameterType == typeof(string))
                {
                    parameters[i] = currentParam.ConvertValue(default);

                    //if (currentParam.GetCustomAttribute<RequestDataBindingAttribute>() is RequestDataBindingAttribute test)
                    //{
                    //}

                    var item = request.UrlQuery.SingleOrDefault(item => item.Name == currentParam.Name);
                    if (item != null)
                    {
                        int paramIndex = Array.IndexOf(paramNames, currentParam.Name);
                        if (paramIndex >= 0)
                        {
                            parameters[paramIndex] = paramInfos[paramIndex].ConvertValue(item.Value);
                        }
                    }
                }
                else if (request.Header.RequestHeaders.ContainsKey(HeaderNames.ContentType))
                {
                    var contentTypeHeader = request.Header.RequestHeaders[HeaderNames.ContentType];
                    //if (contentTypeHeader)
                    //{

                    //}
                    //else if (contentTypeHeader)
                    //{

                    //}
                    //else 
                    if (contentTypeHeader.StartsWith(JsonDeserialize.ContentType))
                    {
                        parameters[i] = await request.ReadJsonPayload(currentParam.ParameterType).ConfigureAwait(false);
                    }
                    else if (contentTypeHeader.StartsWith(XmlDeserialize.ContentType))
                    {
                        parameters[i] = request.ReadXmlPayload(currentParam.ParameterType);
                    }
                    // Can not map single raw object to property (?)
                }

            }

            return parameters;
        }

        private static async Task<ExecutionStatus> Invoke(ExecutedCommand executedCommand, BaseContext context, params object[] parameters)
        {
            if (executedCommand.MethodExecuted is DelegateFactory.ReflectedDelegate reflectedDelegate)
            {
                dynamic data = reflectedDelegate(context, parameters);
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
                return new ExecutionStatus() 
                {
                    ReturnValue = data
                };
            }
            else if (executedCommand.MethodExecuted is DelegateFactory.ReflectedVoidDelegate action)
            {
                action(context, parameters);
            }
            return new ExecutionStatus()
            {
                IsReturnTypeVoid = true
            };
        }
    }

    public class ExecutionStatus
    {
        public bool IsReturnTypeVoid { get; set; }
        public object ReturnValue { get; set; }
    }
}
