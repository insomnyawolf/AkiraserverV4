using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsAsyncMethod(this MethodInfo methodInfo)
        {
            // Obtain the custom attribute for the method.
            if (methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>() is null)
            {
                // Null is returned if the attribute isn't present for the method.
                return false;
            }
            return true;
        }

        public static bool HasProperty(this object objectToCheck, string propertyName)
        {
            var type = objectToCheck.GetType();
            return type.GetProperty(propertyName) != null;
        }


        // https://stackoverflow.com/questions/13071805/dynamic-invoke-of-a-method-using-named-parameters
        public static object InvokeWithNamedParameters(this MethodInfo self, object obj, IDictionary<string, object> namedParameters)
        {
            return self.Invoke(obj, MapParameters(self, namedParameters));
        }

        public static object[] MapParameters(MethodInfo method, IDictionary<string, object> namedParameters)
        {
            string[] paramNames = method.GetParameters().Select(p => p.Name).ToArray();
            object[] parameters = new object[paramNames.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                parameters[i] = Type.Missing;
            }
            foreach (var item in namedParameters)
            {
                var paramName = item.Key;
                var paramIndex = Array.IndexOf(paramNames, paramName);
                if (paramIndex >= 0)
                {
                    parameters[paramIndex] = item.Value;
                }
            }
            return parameters;
        }
    }
}