using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Model;
using System;
using System.Reflection;
using static AkiraserverV4.Http.BaseContex.Context;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private ExecutedCommand DefaultNotFound { get; set; }
        private ExecutedCommand DefaultInternalServerError { get; set; }
        private ExecutedCommand DefaultBadRequest { get; set; }

        private ExecutedCommand notFoundHandler;
        private ExecutedCommand internalServerErrorHandler;
        private ExecutedCommand badRequestHandler;

        private ExecutedCommand NotFoundHandler { get => notFoundHandler ?? DefaultNotFound; set => notFoundHandler = value; }
        private ExecutedCommand InternalServerErrorHandler { get => internalServerErrorHandler ?? DefaultInternalServerError; set => internalServerErrorHandler = value; }
        private ExecutedCommand BadRequestHandler { get => badRequestHandler ?? DefaultBadRequest; set => badRequestHandler = value; }

        public void LoadDefaultRouting()
        {
            Type type = typeof(DefaultContext);
            MethodInfo[] methods = type.GetMethods();
            for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
            {
                MethodInfo currentMethod = methods[methodIndex];
                
                // 400 Handler
                if (currentMethod.GetCustomAttribute<DefaultBadRequestEndpointAttribute>() != null)
                {
                    if (DefaultBadRequest != null)
                    {
                        throw new MultipleMatchException(nameof(DefaultBadRequestEndpointAttribute));
                    }
                    DefaultBadRequest = new ExecutedCommand() { MethodExecuted = currentMethod, ClassExecuted = type };
                }
                // 404 Handler
                if (currentMethod.GetCustomAttribute<DefaultNotFoundEndpointAttribute>() != null)
                {
                    if (DefaultNotFound != null)
                    {
                        throw new MultipleMatchException(nameof(DefaultNotFoundEndpointAttribute));
                    }
                    DefaultNotFound = new ExecutedCommand() { MethodExecuted = currentMethod, ClassExecuted = type };
                }
                // 500 Handler
                else if (currentMethod.GetCustomAttribute<DefaultInternalServerErrorEndpointAttribute>() != null)
                {
                    if (DefaultInternalServerError != null)
                    {
                        throw new MultipleMatchException(nameof(DefaultInternalServerErrorEndpointAttribute));
                    }
                    DefaultInternalServerError = new ExecutedCommand() { MethodExecuted = currentMethod, ClassExecuted = type };
                }
            }

            if (DefaultBadRequest is null)
            {
                throw new InvalidOperationException(nameof(DefaultBadRequest));
            }
            if (DefaultNotFound is null)
            {
                throw new InvalidOperationException(nameof(DefaultNotFound));
            }
            if (DefaultInternalServerError is null)
            {
                throw new InvalidOperationException(nameof(DefaultInternalServerError));
            }
        }
    }
}