﻿using AkiraserverV4.Http.Context.Requests;
using System;
using System.Reflection;
using static Extensions.DelegateFactory;

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
        public ParameterInfo[] ParameterInfo { get; set; }
        public ReflectedDelegate ReflectedDelegate { get; set; }
        public Attribute[] Attributes { get; set; }
    }
}