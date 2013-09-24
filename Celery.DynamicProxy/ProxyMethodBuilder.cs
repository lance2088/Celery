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

            BuildProxyMethodBody(methodInfo, ilGenerator);
        }

        private void BuildProxyMethodBody(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            //if (Interceptor == null)
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
            //Interceptor.Invoke(invocation);

            ilGenerator.DeclareLocal(typeof(IMethodInvocation));

            //this.get_Interceptor();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Callvirt, ReferenceData.InterceptorGetMethod);

            Label jmpThrow = ilGenerator.DefineLabel();

            //if (Interceptor == null)
            ilGenerator.Emit(OpCodes.Dup);
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Bne_Un, jmpThrow);

            //throw new NotImplementedException();
            ilGenerator.Emit(OpCodes.Newobj, ReferenceData.NotImplementedExceptionConstructor);
            ilGenerator.Emit(OpCodes.Throw);

            ilGenerator.MarkLabel(jmpThrow);

            //push this pointer onto stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            //get RuntimeMethodHandle of methodInfo, push onto stack
            ilGenerator.Emit(OpCodes.Ldtoken, methodInfo);  

            // MethodBase.GetMethodFromHandle(runtime(methodInfo));
            ilGenerator.Emit(OpCodes.Callvirt, ReferenceData.GetMethodFromHandle);
            ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));

            //PushGenericArgs(methodInfo, il);
            PushParameters(methodInfo, ilGenerator);

            //IMethodInvocation invocation = new DefaultMethodInvocation(...);
            ilGenerator.Emit(OpCodes.Newobj, 
                ReferenceData.DefaultMethodInvocationConstructor);

            //Interceptor.Invoke(invocation);
            ilGenerator.Emit(OpCodes.Callvirt, 
                ReferenceData.MethodInterceptorInvokeMethod);

            BuildReturnValue(methodInfo, ilGenerator);

            ilGenerator.Emit(OpCodes.Ret);
        }

        private void PushGenericArgs(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            Type[] genericArgs = methodInfo.GetGenericArguments();

            int genArgsCount = 0;
            if (genericArgs != null)
            {
                genArgsCount = genericArgs.Length;
            }
            //Type[] genericTypeArgs = new Type[genArgsCount];
            ilGenerator.Emit(OpCodes.Ldc_I4, genArgsCount);
            ilGenerator.Emit(OpCodes.Newarr, typeof(Type));

            if (genArgsCount > 0)
            {
                for (int i = 0; i < genArgsCount; i++)
                {
                    Type currentType = genericArgs[i];
                    //genericTypeArgs[i] = Type.GetTypeFromHandle(runtime(currentType));
                    ilGenerator.Emit(OpCodes.Dup);
                    ilGenerator.Emit(OpCodes.Ldc_I4, i);
                    ilGenerator.Emit(OpCodes.Ldtoken, currentType);
                    ilGenerator.Emit(OpCodes.Callvirt, ReferenceData.GetTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                }
            }
        }

        private void PushParameters(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            int paramCount = 0;
            if (paramInfos != null)
            {
                paramCount = paramInfos.Length;
            }
            //object[] args = new object[paramCount];
            ilGenerator.Emit(OpCodes.Ldc_I4, paramCount);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));

            if (paramCount == 0)
            {
                //il.Emit(OpCodes.Ldloc_S, 0);
                return;
            }

            ilGenerator.Emit(OpCodes.Stloc_S, 0);

            int i = 0;
            int paramPos = 1;
            foreach (ParameterInfo param in paramInfos)
            {
                Type paramType = param.ParameterType;
                //args[i] = arguments[i]
                ilGenerator.Emit(OpCodes.Ldloc_S, 0); //push the reference of args array onto evaluation stack
                ilGenerator.Emit(OpCodes.Ldc_I4, i);  //push index onto the evaluation stack
                if (param.IsOut)
                {
                    ilGenerator.Emit(OpCodes.Ldnull); //push null onto the evaluation stack
                    //pop the value, index, and array reference, 
                    //the value is put into the array element at given index i
                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                    paramPos++;
                    i++;
                    continue;
                }
                ilGenerator.Emit(OpCodes.Ldarg, paramPos); //load next arg at paramPos position onto evaluation stack
                if (paramType.IsValueType) //if it is value type, box it and push to stack
                    ilGenerator.Emit(OpCodes.Box, paramType);

                //pop the value, index, and array reference, 
                //the value is put into the array element at given index i
                ilGenerator.Emit(OpCodes.Stelem_Ref);

                i++;
                paramPos++;
            }
            ilGenerator.Emit(OpCodes.Ldloc_S, 0);
        }

        private void BuildReturnValue(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Pop);
                return;
            }

            //if the return value is value type, 
            //unbox it and get value and put it on stack.
            if (methodInfo.ReturnType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox, methodInfo.ReturnType);
                if (methodInfo.ReturnType.IsEnum)
                {
                    ilGenerator.Emit(OpCodes.Ldind_I4);
                    return;
                }

                if (methodInfo.ReturnType.IsPrimitive)
                {
                    ilGenerator.Emit(ReferenceData.LdindOpCode[methodInfo.ReturnType]);
                    return;
                }

                ilGenerator.Emit(OpCodes.Ldobj, methodInfo.ReturnType);
            }
        }
    }
}
