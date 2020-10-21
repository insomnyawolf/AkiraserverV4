using AkiraserverV4.Http.Context.Requests;
using System;

namespace AkiraserverV4.Http.Context
{
    public partial class BaseContext
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public sealed class ControllerAttribute : Attribute
        {
            public string Path { get; }

            public ControllerAttribute(string path = null)
            {
                Path = path ?? string.Empty;
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class PostAttribute : BaseEndpointAttribute
        {
            public PostAttribute(string path = null) : base(HttpMethod.Post, path)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class GetAttribute : BaseEndpointAttribute
        {
            public GetAttribute(string path = null) : base(HttpMethod.Get, path)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class RequestAttribute : BaseEndpointAttribute
        {
            public RequestAttribute(string path = null) : base(HttpMethod.Any, path)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class EndpointAttribute : BaseEndpointAttribute
        {
            public EndpointAttribute(HttpMethod method, string path = null) : base(method, path)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public abstract class BaseEndpointAttribute : Attribute
        {
            public HttpMethod Method { get; }
            public string Path { get; }

            protected BaseEndpointAttribute(HttpMethod method = HttpMethod.Any, string path = null)
            {
                Path = path ?? string.Empty;
                Method = method;
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class BadRequestAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class NotFoundHandlerAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal sealed class InternalServerErrorHandlerAttribute : Attribute { }
    }
}