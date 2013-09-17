using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celery.DynamicProxy
{
    public class DebugInterceptor : IMethodInterceptor
    {
        public object Invoke(IMethodInvocation invocation)
        {
            Console.WriteLine("Before: invocation=[{0}]", invocation);
            //在执行该方法时，会递归调用调用链中的所有切入行为
            //最后调用真正的目标方法
            object rval = invocation.Execute();
            Console.WriteLine("Invocation returned");
            return rval;
        }
    }
    public class ProxyFactory
    {
        public virtual Type CreateProxyType(
            Type baseType, params Type[] baseInterfaces)
        {
            ProxyTypeBuilder ptb = 
                new ProxyTypeBuilder(baseType, baseInterfaces);
            Type type = ptb.CreateProxyType();
            return type;
        }
    }
}