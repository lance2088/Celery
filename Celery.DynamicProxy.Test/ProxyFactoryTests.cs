using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Collections;

namespace Celery.DynamicProxy.Test
{
    [TestFixture]
    public class ProxyFactoryTests
    {
        [Test]
        public void DirectCall()
        {
            ProxyFactory factory = new ProxyFactory();
            NopInterceptor interceptor = new NopInterceptor(new ProxyTestClass("test"));
            object proxy = factory.CreateProxy(typeof(ProxyTestClass), interceptor);

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);
            ProxyTestClass proxyTest = proxy as ProxyTestClass;
            Assert.AreEqual("test", proxyTest.Name);
            Assert.AreEqual(1, interceptor.Count);
        }

        [Test]
        public void InterceptVirtualMethod()
        {
            ProxyFactory factory = new ProxyFactory();
            NopInterceptor interceptor = new NopInterceptor(new ProxyTestClass("test"));
            object proxy = factory.CreateProxy(typeof(ProxyTestClass), interceptor);

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);
            ProxyTestClass proxyTest = proxy as ProxyTestClass;
            proxyTest.Name = "test1";
            Assert.AreEqual("test1", proxyTest.Name);
            Assert.AreEqual(2, interceptor.Count);
        }

        [Test]
        public void InterceptVirtualAndOverloadMethod()
        {
            ProxyFactory factory = new ProxyFactory();
            NopInterceptor interceptor = new NopInterceptor(new ProxyTestClass("test", 0));

            object proxy = factory.CreateProxy(typeof(ProxyTestClass), interceptor);

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);
            
            ProxyTestClass proxyTest = proxy as ProxyTestClass;
            Assert.AreEqual(0, proxyTest.Count);

            proxyTest.Dosomething(2);

            Assert.AreEqual(2, proxyTest.Count);
            Assert.AreEqual("test", proxyTest.Name);

            proxyTest.Dosomething("ing");

            Assert.AreEqual("testing", proxyTest.Name);
            Assert.AreEqual(2, proxyTest.Count);
            Assert.AreEqual(7, interceptor.Count);
        }

        [Test]
        public void InterceptInterfaceVirtualMethod()
        {
            ProxyFactory factory = new ProxyFactory();
            NopInterceptor interceptor = new NopInterceptor(new ProxyTestClass("test", 0));

            object proxy = factory.CreateProxy(typeof(ProxyTestClass), interceptor);

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is IProxyTest);
            Assert.IsTrue(proxy is ProxyTestClass);

            IProxyTest proxyTest = proxy as IProxyTest;

            Assert.AreEqual("test", proxyTest.GetName());
            Assert.AreEqual(1, interceptor.Count);
        }
    }

    public interface IProxyTest
    {
        void ReplaceName(string name);
        string GetName();
    }

    public class ProxyTestClass : IProxyTest
    {
        private string _name;
        private int _count;

        public virtual int Count
        {
            get { return _count; }
            set { _count = value; }
        }
        public ProxyTestClass(string name): this(name, 0)
        {
        }

        public ProxyTestClass(string name, int count)
        {
            this._name = name;
            this._count = count;
        }

        public virtual string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public virtual void Dosomething(string str)
        {
            this._name = string.Concat(this._name, str);
        }

        public virtual void Dosomething(int count)
        {
            this._count += count;
        }

        #region IProxyTest Members

        public void ReplaceName(string name)
        {
            this._name = name;
        }

        public virtual string GetName()
        {
            return this._name;
        }

        #endregion
    }

    public class NopInterceptor : IMethodInterceptor
    {
        private object target;
        private int count;

        public int Count
        {
            get { return count; }
        }

        public NopInterceptor(object target)
        {
            this.target = target;
            this.count = 0;
        }
        
        public object Invoke(IMethodInvocation invocation)
        {
            this.count++;
            return invocation.Invoke(target);
        }
    }
}
