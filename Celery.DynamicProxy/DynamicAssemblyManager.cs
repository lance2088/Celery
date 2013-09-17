using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections;

namespace Celery.DynamicProxy
{
    public sealed class DynamicAssemblyManager
    {
        private static readonly IDictionary<string, ModuleBuilder> m_moduleCache =
            new SyncDictionary<string, ModuleBuilder>();

        public static ModuleBuilder GetModuleBuilder(string assemblyName)
        {
            ModuleBuilder moduleBuilder = m_moduleCache[assemblyName];
            if (moduleBuilder == null)
            {

                AssemblyBuilder assemblyBuilder = GetAssemblyBuilder(assemblyName);
#if DEBUG_MODE
                ModuleBuilder moduleBuilder =
                    assemblyBuilder.DefineDynamicModule(an.Name, string.Format("{0}.mod", an.Name), true);
#else
                moduleBuilder =
                    assemblyBuilder.DefineDynamicModule(assemblyName);

                m_moduleCache[assemblyName] = moduleBuilder;
#endif
            }
            return moduleBuilder;
        }

        public static void SaveAssembly(string assemblyName)
        {
            ModuleBuilder moduleBuilder = m_moduleCache[assemblyName];

            if (moduleBuilder == null)
            {
                throw new ArgumentException(
                    string.Format("Argument {0} is not a valid dynamic assembly name", assemblyName),
                    "\"assemblyName\"");
            }

            AssemblyBuilder assembly = (AssemblyBuilder)moduleBuilder.Assembly;
            assembly.Save(assembly.GetName().Name + ".dll");
        }

        private static AssemblyBuilder GetAssemblyBuilder(string assemblyName)
        {
            AssemblyName an = new AssemblyName(assemblyName);
            AppDomain currentDomain = AppDomain.CurrentDomain;

#if DEBUG_MODE
            AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave;
#else
            AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;
#endif
            AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(an, access);

            return assemblyBuilder;
        }
    }
}