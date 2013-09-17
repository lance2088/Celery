using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Celery.DynamicProxy
{
    public interface IMethodInvocation : IInvocation
    {
        MethodInfo Method { get; }
        object Proxy { get; }
        object Target { get; }
        object TargetType { get; }
    }
}