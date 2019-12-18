using AkiraserverV4.Http.ContextFolder.RequestFolder;
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
        public MethodInfo MethodExecuted { get; set; }
        public Type ClassExecuted { get; set; }
    }
}