using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Celery.DynamicProxy
{
    public class DefaultMethodInvocation : IMethodInvocation
    {
        protected object proxy;
        protected object target;
        protected MethodInfo method;
        protected object[] arguments;
        protected Type targetType;

        public DefaultMethodInvocation(
            object proxy,
            object target,
            Type targetType,
            MethodInfo method,
            object[] arguments)
		{
			this.proxy = proxy;
			this.target = target;
            this.targetType = targetType;
            this.method = method;
			this.arguments = arguments;
		}

        #region IMethodInvocation Members

        public MethodInfo Method
        {
            get { return this.method; }
        }

        public object Proxy
        {
            get { return this.proxy; }
        }

        public object Target
        {
            get { return this.target; }
        }

        public object TargetType
        {
            get { return this.targetType; }
        }

        #endregion

        #region IInvocation Members

        public object[] Arguments
        {
            get { return this.arguments; }
        }

        public object Execute()
        {
            try
            {
                return this.method.Invoke(target, arguments);
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
