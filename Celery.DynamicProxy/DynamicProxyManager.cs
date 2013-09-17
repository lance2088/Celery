using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Celery.DynamicProxy
{
    public sealed class DynamicProxyManager
    {
        public const string ASSEMBLY_NAME = "Celery.Proxy";
        public const TypeAttributes PROXY_TYPE_ATTRIBUTES = 
            TypeAttributes.AutoClass | 
            TypeAttributes.Class | 
            TypeAttributes.Public;

        public static TypeBuilder CreateTypeBuilder(string typeName, Type baseType)
        {
            ModuleBuilder moduleBuilder = DynamicAssemblyManager.GetModuleBuilder(ASSEMBLY_NAME);

            Type type = moduleBuilder.GetType(typeName, true);

            if (type != null)
            {
                throw new ArgumentException("Proxy type for \"" + typeName + "\" already existed.");
            }

            TypeBuilder typeBuilder = 
                moduleBuilder.DefineType(typeName, PROXY_TYPE_ATTRIBUTES, baseType);

            return typeBuilder;
        }

        public static void SaveAssembly()
        {
            DynamicAssemblyManager.SaveAssembly(ASSEMBLY_NAME);
        }
    }
}