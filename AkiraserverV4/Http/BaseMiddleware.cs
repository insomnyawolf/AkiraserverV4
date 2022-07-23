using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions.Reflection;
using System;
using System.Reflection;
using System.Threading.Tasks;


namespace AkiraserverV4.Http
{
    public class BaseMiddleware
    {
        protected virtual async Task ActionPreExecuting(BaseContext context, Request request, ExecutedCommand executedCommand) { }

        protected virtual async Task ActionPostExecuting(BaseContext context, Request request, ExecutedCommand executedCommand) { }

        protected virtual async Task<ExecutionStatus> ActionExecuting(BaseContext context, Request request, ExecutedCommand executedCommand)
        {
            return await Next(executedCommand: executedCommand, request: request, context: context);
        }

        protected internal async Task<ExecutionStatus> Next(BaseContext context, Request request, ExecutedCommand executedCommand)
        {
            ExecutionStatus ExecutionStatus;
            // Pre execution
            await ActionPreExecuting(context, request, executedCommand);

            // Execution
            ExecutionStatus = await Invoke(executedCommand: executedCommand, context: context, request: request);

            // Post execution
            await ActionPostExecuting(context, request, executedCommand);

            // return value
            return ExecutionStatus;
        }

        public static async Task<object[]> MapParameters(ExecutedCommand executedCommand, Request request)
        {
#warning rework invoke with named params
            object[] parameters = new object[executedCommand.ParameterInfos.Length];
            for (int parameterIndex = 0; parameterIndex < parameters.Length; ++parameterIndex)
            {
                ParameterInfo currentParam = executedCommand.ParameterInfos[parameterIndex];
                // If object type should try to get the value of the body (json/xml/form) else map query parameters into it

                if (request.HttpHeaders?.ContainsKey(HttpHeaderNames.ContentType) == true)
                {
                    var contentTypeHeader = request.HttpHeaders[HttpHeaderNames.ContentType];

                    if (contentTypeHeader.StartsWith(JsonDeserialize.ContentType))
                    {
                        parameters[parameterIndex] = await request.ReadJsonPayload(currentParam.ParameterType).ConfigureAwait(false);
                    }
                    else if (contentTypeHeader.StartsWith(XmlDeserialize.ContentType))
                    {
                        parameters[parameterIndex] = request.ReadXmlPayload(currentParam.ParameterType);
                    }
                    else
                    {
                        throw new NotImplementedException($"Deserialization for '{contentTypeHeader}'");
                    }
                    // Can not map single raw object to property (?)

                    continue;
                }

                if (!request.Params.TryGetValue(currentParam.Name, out var valueRaw))
                {
                    // no value here
                    parameters[parameterIndex] = default;
                    continue;
                }

                if (!TypeConverter.ConvertTo(valueRaw, executedCommand.ParameterInfos[parameterIndex].ParameterType, out dynamic value))
                {
                    // something failed use bad request here maybe?
                }

                parameters[parameterIndex] = value;
            }

            return parameters;
        }

        private static async Task<ExecutionStatus> Invoke(ExecutedCommand executedCommand, BaseContext context, Request request)
        {
            var parameters = await MapParameters(executedCommand, request);

            var executionStatus = new ExecutionStatus()
            {
                ReturnType = executedCommand.ReflectedDelegate.ReturnType
            };

            var data = executedCommand.ReflectedDelegate.Lambda(context, parameters);

            if (data is Task)
            {
                await data;

                if (executedCommand.ReflectedDelegate.IsGeneric)
                {
                    executionStatus.Value = data.Result;
                }
                else
                {
                    executionStatus.Value = null;
                }
            }
            else
            {
                executionStatus.Value = data;
            }

            return executionStatus;
        }
    }

    public class ExecutionStatus
    {
        public dynamic Value { get; set; }
        public ReturnType ReturnType { get; set; }
    }
}
