using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

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
                    typeof(MethodInfo), 
                    typeof(object[])});

        public static readonly MethodInfo InterceptorGetMethod =
            typeof(IProxy).GetProperty("Interceptor").GetGetMethod();

        public static readonly MethodInfo GetMethodFromHandle =
            typeof(MethodBase).GetMethod(
                "GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
        public static readonly MethodInfo GetTypeFromHandle = 
            typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

        public static readonly MethodInfo MethodInterceptorInvokeMethod = typeof(IMethodInterceptor).GetMethod("Invoke");

        public static readonly IDictionary<Type, OpCode> LdindOpCode = new Dictionary<Type, OpCode> {
            { typeof(sbyte), OpCodes.Ldind_I1 },
            { typeof(short), OpCodes.Ldind_I2 },
            { typeof(int), OpCodes.Ldind_I4 },
            { typeof(long), OpCodes.Ldind_I8 },
            { typeof(byte), OpCodes.Ldind_U1 },
            { typeof(ushort), OpCodes.Ldind_U2 },
            { typeof(uint), OpCodes.Ldind_U4 },
            { typeof(ulong), OpCodes.Ldind_I8 },
            { typeof(float), OpCodes.Ldind_R4 },
            { typeof(double), OpCodes.Ldind_R8 },
            { typeof(bool), OpCodes.Ldind_I1 },
            { typeof(char), OpCodes.Ldind_U2 }
        };
    }
}
