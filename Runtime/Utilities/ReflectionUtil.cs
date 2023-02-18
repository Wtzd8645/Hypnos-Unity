using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hypnos
{
    public static class ReflectionUtil
    {
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