using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Celery.DynamicProxy
{
    public class ProxyMethodBuilder
    {
        public void CreateProxyMethod(
            MethodInfo methodInfo, TypeBuilder typeBuilder)
        {
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            Type[] paramTypes = new Type[paramInfos.Length];
            for (int i = 0; i < paramInfos.Length; i++)
            {
                paramTypes[i] = paramInfos[i].ParameterType;
            }

            MethodAttributes methodAttributes =
                MethodAttributes.Public |
                MethodAttributes.Virtual |
                MethodAttributes.HideBySig;
            
            MethodBuilder methodBuilder = 
                typeBuilder.DefineMethod(
                    methodInfo.Name,
                    methodAttributes,
                    CallingConventions.HasThis, 
                    methodInfo.ReturnType,   
                    paramTypes);

            Type[] genericArgs = methodInfo.GetGenericArguments();

            if (genericArgs != null && genericArgs.Length > 0)
            {
                string[] typeNames = new string[genericArgs.Length];
                for (int i = 0; i < genericArgs.Length; i++)
                {
                    typeNames[i] = "T" + i;
                }
                methodBuilder.DefineGenericParameters(typeNames);
            }

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            BuildProxyMethodBody(ilGenerator, methodInfo);
        }

        private void BuildProxyMethodBody(ILGenerator il, MethodInfo methodInfo)
        {
            //if (Intercepter == null)
            //{
            //    throw new NotImplementedException();
            //}
            //IMethodInvocation invocation =
            //    new DefaultMethodInvocation(
            //        this,
            //        typeof(Test).GetMethod("DoSomething"),
            //        typeof(Test),
            //        null,
            //        null);
            //Intercepter.Invoke(invocation);

            il.DeclareLocal(typeof(IMethodInvocation));

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, ReferenceData.InterceptorGetMethod);

            Label jmpThrow = il.DefineLabel();

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Bne_Un, jmpThrow);

            il.Emit(OpCodes.Newobj, ReferenceData.NotImplementedExceptionConstructor);
            il.Emit(OpCodes.Throw);

            il.MarkLabel(jmpThrow);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldtoken, methodInfo);

            il.Emit(OpCodes.Call, ReferenceData.GetMethodFromHandle);
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));


        }
    }
}
