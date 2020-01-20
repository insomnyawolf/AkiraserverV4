using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// https://stackoverflow.com/questions/13071805/dynamic-invoke-of-a-method-using-named-parameters
        /// </summary>
        /// <param name="self"></param>
        /// <param name="obj"></param>
        /// <param name="namedParameters"></param>
        /// <returns></returns>
        public static object InvokeWithNamedParameters<T>(this MethodInfo self, object obj, IDictionary<string, T> namedParameters) where T : class
        {
            var test = self.MapParameters(namedParameters);
            return self.Invoke(obj, test);
        }

        public static object[] MapParameters<T>(this MethodInfo method, IDictionary<string, T> namedParameters) where T : class
        {
            ParameterInfo[] paramInfos = method.GetParameters().ToArray();
            string[] paramNames = paramInfos.Select(p => p.Name).ToArray();
            object[] parameters = new object[paramNames.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                parameters[i] = paramInfos[i].ConvertValue(default);
            }

            if (namedParameters is null)
            {
                return parameters;
            }
            foreach (var item in namedParameters)
            {
                var paramName = item.Key;
                var paramIndex = Array.IndexOf(paramNames, paramName);
                if (paramIndex >= 0)
                {
                    parameters[paramIndex] = paramInfos[paramIndex].ConvertValue(item.Value);
                }
            }
            return parameters;
        }

        private static readonly Type TypeDateTime = typeof(DateTime);
        private static readonly Type TypeDateTimeNullable = typeof(DateTime?);

        private static readonly Type TypeInt = typeof(int);
        private static readonly Type TypeIntNullable = typeof(int?);

        private static readonly Type TypeDecimal = typeof(decimal);
        private static readonly Type TypeDecimalNullable = typeof(decimal?);

        private static readonly Type TypeBool = typeof(bool);
        private static readonly Type TypeBoolNullable = typeof(bool?);

        private const string CsvDateFormat = "dd/MM/yyyy";

        public static object ConvertValue(this ParameterInfo propertyInfo, object input)
        {
            Type paramType = propertyInfo.ParameterType;
            Type UnderlyingType = Nullable.GetUnderlyingType(paramType);

            string inputStr = (input ?? string.Empty).ToString();

            try
            {
                // Fechas
                if (paramType == TypeDateTime || paramType == TypeDateTimeNullable)
                {
                    bool Sucess = DateTime.TryParseExact(s: inputStr, format: CsvDateFormat, provider: CultureInfo.InvariantCulture, style: DateTimeStyles.None, result: out DateTime Parsed);
                    if (!Sucess && paramType == TypeDateTimeNullable)
                    {
                        return null;
                    }
                    else
                    {
                        return Parsed;
                    }
                }
                else if (paramType == TypeInt || paramType == TypeIntNullable)
                {
                    bool Sucess = int.TryParse(s: inputStr, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out int Parsed);
                    if (!Sucess && paramType == TypeIntNullable)
                    {
                        return null;
                    }
                    else
                    {
                        return Parsed;
                    }
                }
                else if (paramType == TypeDecimal || paramType == TypeDecimalNullable)
                {
                    bool Sucess = decimal.TryParse(s: inputStr, style: NumberStyles.AllowDecimalPoint, provider: CultureInfo.InvariantCulture, result: out decimal Parsed);
                    if (!Sucess && paramType == TypeDecimalNullable)
                    {
                        return null;
                    }
                    else
                    {
                        return Parsed;
                    }
                }
                else if (paramType == TypeBool || paramType == TypeBoolNullable)
                {
                    bool Sucess = bool.TryParse(value: inputStr, result: out bool Parsed);
                    if (!Sucess && paramType == TypeBoolNullable)
                    {
                        return null;
                    }
                    else
                    {
                        return Parsed;
                    }
                }
                else
                {
                    if (UnderlyingType is null)
                    {
                        return Convert.ChangeType(inputStr, paramType);
                    }
                    else
                    {
                        return Convert.ChangeType(inputStr, UnderlyingType);
                    }
                }
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
            {
                if (paramType.IsClass || paramType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return null;
                }
                else
                {
                    return Convert.ChangeType(0, paramType);
                }
            }
        }
    }
}