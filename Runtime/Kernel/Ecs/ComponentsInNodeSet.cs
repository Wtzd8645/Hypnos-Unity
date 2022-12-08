using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morpheus.Ecs
{
    public class ComonentsInNodesSet
    {
        public Dictionary<Type, HashSet<Type>> ComponentsOfNode = new Dictionary<Type, HashSet<Type>>();

        public static ComonentsInNodesSet GetInstance()
        {
            ComonentsInNodesSet set = new ComonentsInNodesSet();
            set.ComponentsOfNode.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                set.AddDataByAssembly(assembly);
            }

            return set;
        }

        public static ComonentsInNodesSet operator +(ComonentsInNodesSet c1, ComonentsInNodesSet c2)
        {
            if (c1 == null)
            {
                return c2;
            }

            foreach (KeyValuePair<Type, HashSet<Type>> typePair in c2.ComponentsOfNode)
            {
                if (!c1.ComponentsOfNode.ContainsKey(typePair.Key))
                {
                    c1.ComponentsOfNode.Add(typePair.Key, typePair.Value);
                }
            }

            return c1;
        }

        private void AddDataByAssembly(Assembly fromAssembly)
        {
            foreach (Type type in fromAssembly.GetTypes()
                .Where(myType =>
                {
                    return myType.IsClass
                        && !myType.IsAbstract
                        && myType.IsSubclassOf(typeof(EcsNode));
                }))
            {
                ComponentsOfNode.Add(type, new HashSet<Type>());

                IEnumerable<PropertyInfo> componentsField = type.GetProperties()
                    .Where((comType) =>
                            comType.PropertyType.IsClass
                            && !comType.PropertyType.IsAbstract
                            && comType.PropertyType.IsSubclassOf(typeof(EcsComponent)));
                foreach (PropertyInfo field in componentsField)
                {
                    ComponentsOfNode[type].Add(field.PropertyType);
                }
            }
        }
    }
}