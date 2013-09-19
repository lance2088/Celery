using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celery.DynamicProxy
{
    public class DebugInterceptor : IMethodInterceptor
    {
        private object target;
        public DebugInterceptor(object target)
        {
            this.target = target;
        }
        public object Invoke(IMethodInvocation invocation)
        {
            Console.WriteLine("Before: invocation=[{0}]", invocation);
            object rval = invocation.Invoke(target);
            Console.WriteLine("Invocation returned");
            return rval;
        }
    }

    public class Test
    {
        public virtual void DoSomething()
        {
            Console.WriteLine("Do something");
        }
    }

    public class Test_Proxy123123123 : Test, IProxy
    {

        public override void DoSomething()
        {
            if (Intercepter == null)
            {
                throw new NotImplementedException();
            }
            IMethodInvocation invocation = 
                new DefaultMethodInvocation(
                    this, 
                    null,
                    typeof(Test),
                    typeof(Test).GetMethod("DoSomething"),
                    null);
            Intercepter.Invoke(invocation);
        }

        #region IProxy Members

        public IMethodInterceptor Intercepter
        {
            get;
            set;
        }

        #endregion
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