﻿using AkiraserverV4.Http;
using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SampleServer.Middlewares
{
    public class Middleware : BaseMiddleware
    {
        private static readonly ILogger<Middleware> Logger = Program.ServiceProvider.GetRequiredService<ILogger<Middleware>>();

        protected override async Task<ExecutionStatus> ActionExecuting(BaseContext context, Request request, ExecutedCommand executedCommand)
        {
            ExecutionStatus ExecutionStatus;
            try
            {
                ExecutionStatus = await Next(context, request, executedCommand);
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception caught by the middleware", ex);
                throw;
            }
            return ExecutionStatus;
        }
    }
}
