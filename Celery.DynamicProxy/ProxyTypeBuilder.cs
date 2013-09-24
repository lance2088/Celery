using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Celery.DynamicProxy
{
    public class ProxyTypeBuilder
    {
        private const string DEFAULT_PROXY_TYPE_NAME = "DynamicProxy";
        private const string FIELD_PREFIX = "__";
        private const string GET_METHOD_PREFIX = "get_";
        private const string SET_METHOD_PREFIX = "set_";

        public Type CreateProxyType(Type baseType, params Type[] interfaces)
        {
            TypeBuilder typeBuilder = 
                CreateProxyTypeBuilder(DEFAULT_PROXY_TYPE_NAME, baseType);
            DefineProxyConstructor(ReferenceData.ObjectConstructor, typeBuilder);

            DefineInterceptor(typeBuilder);

            List<Type> interfaceList = new List<Type>();
            
            if (interfaces != null && interfaces.Length != 0)
            {
                interfaceList.AddRange(interfaces);
            }

            Type[] intfs = interfaceList.ToArray();
            foreach (Type intf in intfs)
            {
                BuildInterfaceList(intf, interfaceList);
            }

            List<MethodInfo> proxyMethodList = new List<MethodInfo>();
            BuildInterfaceMethodList(interfaceList, proxyMethodList);
            BuildInheritClassMethodList(baseType, proxyMethodList);

            ProxyMethodBuilder proxyMethodBuilder = new ProxyMethodBuilder();

            foreach (MethodInfo method in proxyMethodList)
            {
                proxyMethodBuilder.CreateProxyMethod(method, typeBuilder);
            }

            Type proxyType = typeBuilder.CreateType();

#if DEBUG_MODE
            DynamicProxyManager.SaveAssembly();
#endif
            return proxyType;
        }

        private void DefineProxyConstructor(
            ConstructorInfo ctorInfo, TypeBuilder typeBuilder)
        {
            MethodAttributes constructorAttributes = 
                MethodAttributes.Public |
                MethodAttributes.HideBySig | 
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName;

            ConstructorBuilder ctorBuilder = 
                typeBuilder.DefineConstructor(
                    constructorAttributes, 
                    CallingConventions.Standard,
                    new Type[] { });

            ILGenerator ilGenerator = ctorBuilder.GetILGenerator();

            ctorBuilder.SetImplementationFlags(
                MethodImplAttributes.IL | MethodImplAttributes.Managed);

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Callvirt, ctorInfo);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private FieldInfo DefineField(
            string fieldName, Type fieldType, TypeBuilder typeBuilder)
        {
            FieldInfo field =
               typeBuilder.DefineField(
                   fieldName,
                   fieldType,
                   FieldAttributes.Private);

            return field;
        }

        private MethodBuilder DefineGetterProperty(
            string propertyName,
            Type propertyType,
            TypeBuilder typeBuilder
            )
        {
            string fieldName =
                string.Format("{0}{1}", FIELD_PREFIX, propertyName.ToLower());
            FieldInfo field = typeBuilder.GetField(fieldName);
            if (field == null)
            {
                field = DefineField(fieldName, propertyType, typeBuilder);
            }

            MethodAttributes attributes =
                MethodAttributes.Public |
                MethodAttributes.Virtual |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot;

            string getMethodName =
                string.Format("{0}{1}", GET_METHOD_PREFIX, propertyName);

            MethodBuilder getter =
                typeBuilder.DefineMethod(
                    getMethodName,
                    attributes,
                    CallingConventions.HasThis,
                    propertyType,
                    new Type[0]);

            getter.SetImplementationFlags(
                MethodImplAttributes.Managed | MethodImplAttributes.IL);

            ILGenerator ilGenerator = getter.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            return getter;
        }

        private MethodBuilder DefineSetterProperty(
            string propertyName,
            Type propertyType,
            TypeBuilder typeBuilder
            )
        {
            string fieldName =
                string.Format("{0}{1}", FIELD_PREFIX, propertyName.ToLower());

            string getMethodName =
                string.Format("{0}{1}", GET_METHOD_PREFIX, propertyName);

            FieldInfo field = typeBuilder.GetField(fieldName);
            if (field == null)
            {
                field = DefineField(fieldName, propertyType, typeBuilder);
            }

            MethodAttributes attributes =
                MethodAttributes.Public |
                MethodAttributes.Virtual |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot;

            MethodBuilder getter =
                typeBuilder.DefineMethod(
                    getMethodName,
                    attributes,
                    CallingConventions.Standard,
                    propertyType,
                    new Type[0]);

            getter.SetImplementationFlags(
                MethodImplAttributes.Managed | MethodImplAttributes.IL);

            ILGenerator ilGenerator = getter.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            string setMethodName =
                string.Format("{0}{1}", SET_METHOD_PREFIX, propertyName);

            MethodBuilder setter =
                typeBuilder.DefineMethod(
                    setMethodName,
                    attributes,
                    CallingConventions.HasThis,
                    typeof(void),
                    new Type[] { propertyType });

            setter.SetImplementationFlags(
                MethodImplAttributes.Managed | MethodImplAttributes.IL);

            ilGenerator = setter.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            return setter;
        }

        private TypeBuilder CreateProxyTypeBuilder(string name, Type baseType)
        {
            string typeName = 
                String.Format("{0}_{1}", name, Guid.NewGuid().ToString("N"));

            return DynamicProxyManager.CreateTypeBuilder(typeName, baseType);
        }

        private void BuildInterfaceList(Type currentType, List<Type> interfaceList)
        {
            Type[] interfaces = currentType.GetInterfaces();
            if (interfaces == null || interfaces.Length == 0) return;

            foreach (Type current in interfaces)
            {
                if (!interfaceList.Contains(current))
                {
                    interfaceList.Add(current);
                    BuildInterfaceList(current, interfaceList);
                }
            }
        }

        private void BuildInterfaceMethodList(
            IList<Type> interfaces,
            IList<MethodInfo> proxyMethods)
        {
            foreach (Type intf in interfaces)
            {
                MethodInfo[] methods = intf.GetMethods();
                foreach(MethodInfo method in methods)
                {
                    if (!proxyMethods.Contains(method))
                    {
                        proxyMethods.Add(method);
                    }
                }
            }
        }

        private void BuildInheritClassMethodList(
            Type baseType, 
            IList<MethodInfo> proxyMethods)
        {
            MethodInfo[] methods = 
                baseType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo method in methods)
            {
                if (method.IsPrivate || method.IsFinal) continue;
                if (method.IsVirtual || method.IsAbstract)
                {
                    proxyMethods.Add(method);
                }
            }
        }

        private void DefineInterceptor(TypeBuilder typeBuilder)
        {
            typeBuilder.AddInterfaceImplementation(typeof(IProxy));

            MethodBuilder getterBody = DefineGetterProperty("Interceptor", typeof(IMethodInterceptor), typeBuilder);
            MethodBuilder setterBody = DefineSetterProperty("Interceptor", typeof(IMethodInterceptor), typeBuilder);

            
            MethodInfo getterDeclaration = typeof(IProxy).GetMethod("get_Interceptor");
            MethodInfo setterDeclaration = typeof(IProxy).GetMethod("set_Interceptor");

            typeBuilder.DefineMethodOverride(getterBody, getterDeclaration);
            typeBuilder.DefineMethodOverride(setterBody, setterDeclaration);
        }
    }
}