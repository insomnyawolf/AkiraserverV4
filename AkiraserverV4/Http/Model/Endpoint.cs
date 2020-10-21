using AkiraserverV4.Http.Context.Requests;
using System;
using System.Reflection;

namespace AkiraserverV4.Http.Model
{
    // Input Info
    public class Endpoint : ExecutedCommand
    {
        public string Path { get; set; }
        public HttpMethod Method { get; set; }
        public int Priority { get; set; }

    }

    // What to execute
    public class ExecutedCommand
    {
        public ParameterInfo[] ParameterInfo { get; set; }
        public object MethodExecuted { get; set; }
        public Attribute[] Attributes { get; set; }
        public Type ClassExecuted { get; set; }
        public bool ReturnIsGenericType { get; set; }

    }
}