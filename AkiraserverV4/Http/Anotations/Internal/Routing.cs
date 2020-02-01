using System;

namespace AkiraserverV4.Http.BaseContext
{
    public abstract partial class Context
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal sealed class DefaultNotFoundEndpointAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal sealed class DefaultBadRequestEndpointAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal sealed class DefaultInternalServerErrorEndpointAttribute : Attribute { }
    }
}