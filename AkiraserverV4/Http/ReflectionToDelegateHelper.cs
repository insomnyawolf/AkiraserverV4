using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AkiraserverV4.Http
{
    public static class DelegateFactory
    {
        private static readonly Type TypeOfVoid = typeof(void);
        private static readonly Type TypeOfObject = typeof(object);
        private static readonly Type TypeOfObjectArray = typeof(object[]);

        public delegate object ReflectedDelegate(object target, params object[] arguments);
        public delegate void ReflectedVoidDelegate(object target, params object[] arguments);

        /// <summary>
        /// Creates a LateBoundMethod delegate from a MethodInfo structure
        /// Basically creates a dynamic delegate on the fly.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static object CreateReflectedDelegate(this MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(TypeOfObject, "target");
            ParameterExpression argumentsParameter = Expression.Parameter(TypeOfObjectArray, "arguments");

            var call = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                CreateParameterExpressions(method, argumentsParameter)
            );

            if (method.ReturnType == TypeOfVoid)
            {
                return Expression.Lambda<ReflectedVoidDelegate>(
                            call,
                            instanceParameter,
                            argumentsParameter
                        ).Compile();
            }
            else
            {
                return Expression.Lambda<ReflectedDelegate>(
                            Expression.Convert(call, TypeOfObject),
                            instanceParameter,
                            argumentsParameter
                        ).Compile();
            }
        }

        /// <summary>
        /// Creates a LateBoundMethod from type methodname and parameter signature that
        /// is turned into a MethodInfo structure and then parsed into a dynamic delegate
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public static object Create(Type type, string methodName, params Type[] parameterTypes)
        {
            return type.GetMethod(methodName, parameterTypes).CreateReflectedDelegate();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
                    parameter.ParameterType)
                ).ToArray();
        }
    }
}
