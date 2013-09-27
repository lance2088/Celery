using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celery.DynamicProxy
{
    //public class DebugInterceptor : IMethodInterceptor
    //{
    //    private object target;
    //    public DebugInterceptor(object target)
    //    {
    //        this.target = target;
    //    }
    //    public object Invoke(IMethodInvocation invocation)
    //    {
    //        Console.WriteLine("Before: invocation=[{0}]", invocation);
    //        object rval = invocation.Invoke(target);
    //        Console.WriteLine("Invocation returned");
    //        return rval;
    //    }
    //}

    //public class Test
    //{
    //    public virtual IList<int> DoSomething(IList<int> x)
    //    {
    //        return x;
    //    }
    //}

    //public class Test_Proxy123123123 : Test, IProxy
    //{

    //    public override IList<int> DoSomething(IList<int> x)
    //    {
    //        if (Interceptor == null)
    //        {
    //            throw new NotImplementedException();
    //        }
    //        IMethodInvocation invocation = 
    //            new DefaultMethodInvocation(
    //                this, 
    //                typeof(Test).GetMethod("DoSomething"),
    //                new object[0]);
    //        return (IList<int>)Interceptor.Invoke(invocation);
    //    }

    //    #region IProxy Members

    //    public IMethodInterceptor Interceptor
    //    {
    //        get;
    //        set;
    //    }

    //    #endregion
    //}

    public class ProxyFactory
    {
        public Type CreateProxyType(
            Type baseType, params Type[] baseInterfaces)
        {
            ProxyTypeBuilder ptb = 
                new ProxyTypeBuilder();
            Type type = ptb.CreateProxyType(baseType, baseInterfaces);
            return type;
        }

        public T CreateProxy<T>(
            IMethodInterceptor interceptor, 
            params Type[] baseInterfaces)
        {
            Type proxyType = CreateProxyType(typeof(T), baseInterfaces);
            T result = (T)Activator.CreateInstance(proxyType);

            IProxy proxy = (IProxy)result;
            proxy.Interceptor = interceptor;

            return result;
        }

        public object CreateProxy(
            Type instanceType, 
            IMethodInterceptor interceptor, 
            params Type[] baseInterfaces)
        {
            Type proxyType = CreateProxyType(instanceType, baseInterfaces);
            object result = Activator.CreateInstance(proxyType);
            IProxy proxy = (IProxy)result;
            
            proxy.Interceptor = interceptor;

            return result;
        }
    }
}