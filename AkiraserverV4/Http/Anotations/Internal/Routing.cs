using AkiraserverV4.Http.BaseContex.Requests;
using System;

namespace AkiraserverV4.Http.BaseContex
{
    public abstract partial class BaseContext
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal sealed class DefaultNotFoundEndpointAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal sealed class DefaultInternalServerErrorEndpointAttribute : Attribute { }
    }
}