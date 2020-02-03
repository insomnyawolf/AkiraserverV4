using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.BaseContext
{
    public abstract partial class Context
    {
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
        public sealed class InputUrlEncodedFormAttribute : RequestDataBindingAttribute { }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
        public abstract class RequestDataBindingAttribute : Attribute { }
    }
}