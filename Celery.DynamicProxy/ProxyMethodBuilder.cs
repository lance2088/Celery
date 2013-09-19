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

        private void BuildProxyMethodBody(MethodInfo methodInfo, ILGenerator il)
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

            //this.get_Interceptor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, ReferenceData.InterceptorGetMethod);

            Label jmpThrow = il.DefineLabel();

            //if (Intercepter == null)
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Bne_Un, jmpThrow);

            //throw new NotImplementedException();
            il.Emit(OpCodes.Newobj, ReferenceData.NotImplementedExceptionConstructor);
            il.Emit(OpCodes.Throw);

            il.MarkLabel(jmpThrow);

            //push this pointer onto stack
            il.Emit(OpCodes.Ldarg_0);
            //get RuntimeMethodHandle of methodInfo, push onto stack
            il.Emit(OpCodes.Ldtoken, methodInfo);  

            // MethodBase.GetMethodFromHandle(runtime(methodInfo));
            il.Emit(OpCodes.Call, ReferenceData.GetMethodFromHandle);
            il.Emit(OpCodes.Castclass, typeof(MethodInfo));

            //PushGenericArgs(methodInfo, il);
            PushParameters(methodInfo, il);

            //IMethodInvocation invocation = new DefaultMethodInvocation(...);
            il.Emit(OpCodes.Newobj, 
                ReferenceData.DefaultMethodInvocationConstructor);


        }

        private void PushGenericArgs(MethodInfo methodInfo, ILGenerator il)
        {
            Type[] genericArgs = methodInfo.GetGenericArguments();

            int genArgsCount = 0;
            if (genericArgs != null)
            {
                genArgsCount = genericArgs.Length;
            }
            //Type[] genericTypeArgs = new Type[genArgsCount];
            il.Emit(OpCodes.Ldc_I4, genArgsCount);
            il.Emit(OpCodes.Newarr, typeof(Type));

            if (genArgsCount > 0)
            {
                for (int i = 0; i < genArgsCount; i++)
                {
                    Type currentType = genericArgs[i];
                    //genericTypeArgs[i] = Type.GetTypeFromHandle(runtime(currentType));
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldtoken, currentType);
                    il.Emit(OpCodes.Call, ReferenceData.GetTypeFromHandle);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
        }

        private void PushParameters(MethodInfo methodInfo, ILGenerator il)
        {
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            int paramCount = 0;
            if (paramInfos != null)
            {
                paramCount = paramInfos.Length;
            }
            //object[] args = new object[paramCount];
            il.Emit(OpCodes.Ldc_I4, paramCount);
            il.Emit(OpCodes.Newarr, typeof(object));

            if (paramCount == 0)
            {
                //il.Emit(OpCodes.Ldloc_S, 0);
                return;
            }

            il.Emit(OpCodes.Stloc_S, 0);

            int i = 0;
            int paramPos = 1;
            foreach (ParameterInfo param in paramInfos)
            {
                Type paramType = param.ParameterType;
                // args[i] = arguments[i]
                il.Emit(OpCodes.Ldloc_S, 0); //push the reference of args array onto evaluation stack
                il.Emit(OpCodes.Ldc_I4, i);  //push index onto the evaluation stack
                if (param.IsOut)
                {
                    il.Emit(OpCodes.Ldnull); //push null onto the evaluation stack
                    //pop the value, index, and array reference, 
                    //the value is put into the array element at given index i
                    il.Emit(OpCodes.Stelem_Ref);
                    paramPos++;
                    i++;
                    continue;
                }
                il.Emit(OpCodes.Ldarg, paramPos); //load next arg at paramPos position onto evaluation stack
                if (paramType.IsValueType) //if it is value type, box it and push to stack
                    il.Emit(OpCodes.Box, paramType);

                //pop the value, index, and array reference, 
                //the value is put into the array element at given index i
                il.Emit(OpCodes.Stelem_Ref);

                i++;
                paramPos++;
            }
            il.Emit(OpCodes.Ldloc_S, 0);
        }
    }
}
