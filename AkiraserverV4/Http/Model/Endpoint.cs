using AkiraserverV4.Http.Context.Requests;
using Extensions;
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
        public SpecialEndpoint SpecialEndpoint { get; set; }

    }

    public enum SpecialEndpoint
    {
        No,
        BadRequest,
        NotFound,
        InternalServerError
    }

    // What to execute
    public class ExecutedCommand
    {
        public ParameterInfo[] ParameterInfos => ReflectedDelegate.ParameterInfos;
        public ReflectedDelegate ReflectedDelegate { get; set; }
        public Attribute[] Attributes { get; set; }
    }
}