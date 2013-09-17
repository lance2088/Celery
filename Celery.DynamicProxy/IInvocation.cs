using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celery.DynamicProxy
{
    public interface IInvocation
    {
        object[] Arguments { get; }
        object Execute();
    }
}