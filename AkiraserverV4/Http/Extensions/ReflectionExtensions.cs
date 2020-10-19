using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using AkiraserverV4.Http.BaseContext.Requests;
using static AkiraserverV4.Http.BaseContext.Ctx;
using AkiraserverV4.Http.BaseContext;

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
        public static object InvokeWithNamedParametersFromContext<T>(this MethodInfo self, BaseContext.Ctx context) where T : class
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

                // If object type should try to get the value of the body (json/xml/form) else map query parameters into it


                if(    currentParam.ParameterType == typeof(byte)
                    || currentParam.ParameterType == typeof(byte?)
                    || currentParam.ParameterType == typeof(sbyte)
                    || currentParam.ParameterType == typeof(sbyte?)
                    || currentParam.ParameterType == typeof(short)
                    || currentParam.ParameterType == typeof(short?)
                    || currentParam.ParameterType == typeof(ushort)
                    || currentParam.ParameterType == typeof(ushort?)
                    || currentParam.ParameterType == typeof(int)
                    || currentParam.ParameterType == typeof(int?)
                    || currentParam.ParameterType == typeof(uint)
                    || currentParam.ParameterType == typeof(uint?)
                    || currentParam.ParameterType == typeof(long)
                    || currentParam.ParameterType == typeof(long?)
                    || currentParam.ParameterType == typeof(ulong)
                    || currentParam.ParameterType == typeof(ulong?)
                    || currentParam.ParameterType == typeof(float)
                    || currentParam.ParameterType == typeof(float?)
                    || currentParam.ParameterType == typeof(double)
                    || currentParam.ParameterType == typeof(double?)
                    || currentParam.ParameterType == typeof(char)
                    || currentParam.ParameterType == typeof(char?)
                    || currentParam.ParameterType == typeof(decimal)
                    || currentParam.ParameterType == typeof(decimal?)
                    || currentParam.ParameterType == typeof(bool)
                    || currentParam.ParameterType == typeof(bool?)
                    || currentParam.ParameterType == typeof(DateTime)
                    || currentParam.ParameterType == typeof(DateTime?)
                    || currentParam.ParameterType == typeof(string))
                {
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
                else
                {
                    // Map body Object Here
                }
                
            }

            return parameters;
        }
    }
}