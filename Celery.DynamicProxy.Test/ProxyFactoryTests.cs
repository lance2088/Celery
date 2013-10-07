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
            ProxyTestClass test = new ProxyTestClass("test");
            object proxy = factory.CreateProxy(typeof(ProxyTestClass), new NopInterceptor(test));

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);
            ProxyTestClass proxyTest = proxy as ProxyTestClass;
            Assert.AreEqual("test", proxyTest.Name);
        }

        [Test]
        public void InterceptVirtualMethod()
        {
            ProxyFactory factory = new ProxyFactory();
            ProxyTestClass test = new ProxyTestClass("test");
            object proxy = factory.CreateProxy(typeof(ProxyTestClass), new NopInterceptor(test));

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);
            ProxyTestClass proxyTest = proxy as ProxyTestClass;
            proxyTest.Name = "test1";
            Assert.AreEqual("test1", proxyTest.Name);
        }

        [Test]
        public void InterceptVirtualMethodAndAmbiguousMatches()
        {
            ProxyFactory factory = new ProxyFactory();
            ProxyTestClass test = new ProxyTestClass("test", 0);

            object proxy = factory.CreateProxy(typeof(ProxyTestClass), new NopInterceptor(test));

            Assert.IsTrue(proxy is IProxy);
            Assert.IsTrue(proxy is ProxyTestClass);

            ProxyTestClass proxyTest = proxy as ProxyTestClass;

            Assert.AreEqual(0, proxyTest.Count);

            proxyTest.Dosomething(2);

            //Assert.AreEqual(2, proxyTest.Count);
            Assert.AreEqual("test", proxyTest.Name);

            proxyTest.Dosomething("ing");

            Assert.AreEqual("testing", proxyTest.Name);
            //Assert.AreEqual(2, proxyTest.Count);

            //ProxyTestClass c = new ProxyTestClassEx("", 0);
            //c.Dosomething(5);

            //Assert.AreEqual(5, c.Count);
        }
    }

    public class ProxyTestClassEx : ProxyTestClass
    {

        public ProxyTestClassEx(string name, int count) : base(name, count) { }
        public override void Dosomething(int count)
        {
            base.Dosomething(count);
        }
    }

    public class ProxyTestClass
    {
        private string _name;
        private int _count;

        public int Count
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

        public virtual string GetName()
        {
            return this._name;
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
