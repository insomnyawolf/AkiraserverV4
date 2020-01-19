using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private async Task InvokeHandlerAsync(Context context, ExecutedCommand executedCommand, Exception exception = null)
        {
            
            dynamic data = InvokeNamedParams(context, executedCommand, exception);

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

        private async Task InvokeNamedParams(Context context, ExecutedCommand executedCommand, Exception exception = null)
        {
            var parameters = new Dictionary<string, object>();

            if (exception != null)
            {
                parameters.Add("exception", exception);
            }

            executedCommand.MethodExecuted.InvokeWithNamedParameters(context, parameters);
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