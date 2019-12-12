using System;
using System.Collections.Generic;
using System.Text;

namespace AkiraserverV4.Http.Anotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DefaultRoutingAttribute : Attribute { }
}
