using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.BaseContex
{
    public abstract partial class Context
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public sealed class InputUrlEncodedFormAttribute : Attribute { }
    }
}
