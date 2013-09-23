using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Celery.DynamicProxy
{
    internal struct ReferenceData
    {
        public static readonly ConstructorInfo ObjectConstructor =
            typeof(object).GetConstructor(new Type[0]);

        public static readonly ConstructorInfo NotImplementedExceptionConstructor =
            typeof(NotImplementedException).GetConstructor(new Type[0]);

        public static readonly ConstructorInfo DefaultMethodInvocationConstructor =
            typeof(DefaultMethodInvocation).GetConstructor(
                new Type[] { 
                    typeof(object), 
                    typeof(object), 
                    typeof(Type),
                    typeof(MethodInfo), 
                    typeof(object[])});

        public static readonly MethodInfo InterceptorGetMethod =
            typeof(IProxy).GetProperty("Interceptor").GetGetMethod();

        public static readonly MethodInfo GetMethodFromHandle =
            typeof(MethodBase).GetMethod(
                "GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
        public static readonly MethodInfo GetTypeFromHandle = 
            typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

        public static MethodInfo MethodInterceptorInvokeMethod = typeof(IMethodInterceptor).GetMethod("Invoke");
    }
}
