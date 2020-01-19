using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.BaseContex;
using Microsoft.Extensions.Logging;
using System.IO;
using AkiraserverV4.Http.Extensions;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private async Task InvokeHandlerAsync(Context context, ExecutedCommand executedCommand, params object[] args)
        {
            dynamic data = executedCommand.MethodExecuted.Invoke(context, args);

            if (data is Task)
            {
                await data;

                if (executedCommand.MethodExecuted.ReturnType.IsGenericType)
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
                await context.SendJson(jsonSerializable);
            }
            else if (data is object)
            {
                await context.SendText(data);
            }
        }
    }
}