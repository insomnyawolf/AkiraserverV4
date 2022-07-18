﻿using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Extensions;
using TypeConverterHelper;

namespace AkiraserverV4.Http
{
    public class BaseMiddleware
    {
        public BaseContext Context { get; set; }

        public virtual async Task<ExecutionStatus> ActionExecuting(ExecutedCommand executedCommand)
        {
            return await InvokeNamedParams(Context, executedCommand).ConfigureAwait(false);
        }

        public static async Task<ExecutionStatus> InvokeNamedParams(BaseContext context, ExecutedCommand executedCommand)
        {
            var parameters = await MapParameters(executedCommand, context.Request).ConfigureAwait(false);

            return await Invoke(executedCommand: executedCommand, context: context, parameters: parameters).ConfigureAwait(false);
        }

        public static async Task<object[]> MapParameters(ExecutedCommand executedCommand, Request request)
        {
#warning rework invoke with named params
            object[] parameters = new object[executedCommand.ParameterInfo.Length];
            for (int parameterIndex = 0; parameterIndex < parameters.Length; ++parameterIndex)
            {
                ParameterInfo currentParam = executedCommand.ParameterInfo[parameterIndex];
                // If object type should try to get the value of the body (json/xml/form) else map query parameters into it

                if (request.Header.RequestHeaders.ContainsKey(HeaderNames.ContentType))
                {
                    var contentTypeHeader = request.Header.RequestHeaders[HeaderNames.ContentType];

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

                if (!TypeConverter.ConvertTo(valueRaw, executedCommand.ParameterInfo[parameterIndex].ParameterType, out dynamic value))
                {
                    // something failed use bad request here maybe?
                }

                parameters[parameterIndex] = value;
            }

            return parameters;
        }

        private static async Task<ExecutionStatus> Invoke(ExecutedCommand executedCommand, BaseContext context, params object[] parameters)
        {
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
