using AkiraserverV4.Http.Context.Requests;
using AkiraserverV4.Http.Context.Responses;
using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Extensions;
using AkiraserverV4.Http.Helper;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AkiraserverV4.Http.Context
{
    public partial class BaseContext
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public NetworkStream NetworkStream { get; private set; }

        private bool HeadersWritten;

        public BaseContext() { }

        internal async Task WriteBodyAsync()
        {
            var data = Response.Body;
            if (data is null)
            {
                if (Response.Status == HttpStatus.Ok)
                {
                    Response.Status = HttpStatus.NoContent;
                }
            }
            else if (data is ResponseResult responseResult)
            {
                await SendResponseResultAsync(responseResult).ConfigureAwait(false);
            }
            else if (data is BinaryResponseResult binaryResponse)
            {
                await binaryResponse.CustomResponse(this).ConfigureAwait(false);
                await WriteDataAsync(binaryResponse.Content).ConfigureAwait(false);

            }
            else if (data is MemoryStream stream)
            {
                await WriteDataAsync(stream).ConfigureAwait(false);
            }
            else if (data is object)
            {
                await SendTextAsync(data).ConfigureAwait(false);
            }
        }

        public async Task WriteDataAsync(byte[] data)
        {
            await NetworkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await NetworkStream.FlushAsync().ConfigureAwait(false);
        }

        public async Task WriteDataAsync(MemoryStream data)
        {
            await WriteHeadersAsync().ConfigureAwait(false);

            data.Position = 0;
            await data.CopyToAsync(NetworkStream).ConfigureAwait(false);
            await NetworkStream.FlushAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the current headers into the network stream if they were not written before
        /// </summary>
        /// <returns></returns>
        public async Task WriteHeadersAsync()
        {
            if (!HeadersWritten)
            {
                byte[] headers = Response.ProcessHeaders().ToByteArray();

                await NetworkStream.WriteAsync(headers, 0, headers.Length).ConfigureAwait(false);

                HeadersWritten = true;
            }
        }

        internal async Task SendResponseResultAsync<T>(T data) where T : ResponseResult
        {
            Response.AddContentType(data.ContentType);
            await SendTextAsync(data.Serialize()).ConfigureAwait(false);
        }

        internal async Task SendTextAsync(object input)
        {
#warning Moove to constants / enum

            Response.AddContentType(ContentType.PlainText);
            byte[] responseBytes = Encoding.UTF8.GetBytes(Convert.ToString(input));
            Response.AddContentLenght(responseBytes.Length);
            await WriteDataAsync(responseBytes).ConfigureAwait(false);
        }

        internal async Task SendRawAsync(object data)
        {
            using (MemoryStream dataStream = data.ToStream())
            {
                Response.AddContentLenght(Convert.ToInt32(dataStream.Length));
#warning Moove to constants / enum
                Response.AddContentType(ContentType.Binary);

                await WriteDataAsync(dataStream).ConfigureAwait(false);
            }
        }

        public async Task<object> InvokeHandlerAsync(ExecutedCommand executedCommand)
        {
            dynamic data = await InvokeNamedParams(this, executedCommand).ConfigureAwait(false);

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

        private static async Task<object> InvokeNamedParams(BaseContext context, ExecutedCommand executedCommand)
        {
            var parameters = await MapParameters(executedCommand.ParameterInfo, context.Request).ConfigureAwait(false);

            return Invoke(methodExecuted: executedCommand.MethodExecuted, context: context, parameters: parameters);
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

        private static object Invoke(object methodExecuted, BaseContext context, params object[] parameters)
        {
            if (methodExecuted is DelegateFactory.ReflectedDelegate reflectedDelegate)
            {
                return reflectedDelegate(context, parameters);
            }
            else if (methodExecuted is DelegateFactory.ReflectedVoidDelegate action)
            {
                action(context, parameters);
            }
            return null;
        }
    }
}