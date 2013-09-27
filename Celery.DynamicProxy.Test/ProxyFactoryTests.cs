using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Celery.DynamicProxy.Test
{
    [TestFixture]
    public class ProxyFactoryTests
    {
        [Test]
        public void DirectCall()
        {
            ProxyFactory factory = new ProxyFactory();
            ProxyTestClass test = new ProxyTestClass("test");
            object proxy = factory.CreateProxy(typeof(ProxyTestClass), new NopInterceptor(test));

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);
            Assert.AreEqual("test", ((ProxyTestClass)proxy).GetName());
        }
    }

    public class ProxyTestClass
    {
        private string _name;
        public ProxyTestClass(string name)
        {
            this._name = name;
        }

        public virtual string GetName()
        {
            return this._name;
        }
    }

    public class NopInterceptor : IMethodInterceptor
    {
        private object target;
        public NopInterceptor(object target)
        {
            this.target = target;
        }
        public object Invoke(IMethodInvocation invocation)
        {
            return invocation.Invoke(target);
        }
    }
}
