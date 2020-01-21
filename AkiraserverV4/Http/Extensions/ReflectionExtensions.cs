using AkiraserverV4.Http.BaseContex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using static AkiraserverV4.Http.BaseContex.Context;
using AkiraserverV4.Http.BaseContex.Requests;

namespace AkiraserverV4.Http.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// https://stackoverflow.com/questions/13071805/dynamic-invoke-of-a-method-using-named-parameters
        /// </summary>
        /// <param name="self"></param>
        /// <param name="obj"></param>
        /// <param name="namedParameters"></param>
        /// <returns></returns>
        public static object InvokeWithNamedParametersFromContext<T>(this MethodInfo self, Context context) where T : class
        {
            object[] test = self.MapParameters(context.Request);
            return self.Invoke(context, test);
        }

        public static object[] MapParameters(this MethodInfo method, Request request)
        {
            ParameterInfo[] paramInfos = method.GetParameters().ToArray();
            string[] paramNames = paramInfos.Select(p => p.Name).ToArray();
            object[] parameters = new object[paramNames.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo currentParam = paramInfos[i];
                parameters[i] = currentParam.ConvertValue(default);

                if (currentParam.GetCustomAttribute<RequestDataBindingAttribute>() is RequestDataBindingAttribute test)
                {
                    if (request.UrlQuery.TryGetValue(currentParam.Name, out string value))
                    {
                        int paramIndex = Array.IndexOf(paramNames, currentParam.Name);
                        if (paramIndex >= 0)
                        {
                            parameters[paramIndex] = paramInfos[paramIndex].ConvertValue(value);
                        }
                    }
                }
            }

            return parameters;
        }
    }
}