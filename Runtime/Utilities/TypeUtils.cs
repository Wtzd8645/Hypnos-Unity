using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blanketmen.Hypnos
{
    public static class TypeUtils
    {
        public static bool IsFinalAssignableType(Type type) => !type.IsAbstract && !type.IsInterface;

        public static Type GetTypeFromFullName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            string[] names = typeName.Split(' ');
            Assembly asm = Assembly.Load(names[0]);
            Type type = asm.GetType(names[1]);
            return type;
        }

        public static void GetTypesFromAssembly(List<Type> result, Type baseType, Assembly asm)
        {
            if (asm == null)
            {
                return;
            }

            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                if (types[i].BaseType == baseType)
                {
                    result.Add(types[i]);
                }
            }
        }
    }
}