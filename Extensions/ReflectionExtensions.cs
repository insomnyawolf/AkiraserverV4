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
    }
}