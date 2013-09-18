using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Celery.DynamicProxy
{
    internal struct ReferenceData
    {
        public static MethodInfo InterceptorGetMethod =
            typeof(IProxy).GetProperty("Interceptor").GetGetMethod();

        public static readonly ConstructorInfo ObjectConstructor =
            typeof(object).GetConstructor(new Type[0]);

        public static ConstructorInfo NotImplementedExceptionConstructor =
            typeof(NotImplementedException).GetConstructor(new Type[0]);

        public static MethodInfo GetMethodFromHandle =
            typeof(MethodBase).GetMethod(
                "GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });

    }
}
