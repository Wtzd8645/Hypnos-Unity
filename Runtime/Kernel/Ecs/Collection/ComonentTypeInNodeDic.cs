using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morpheus.Ecs
{
    public class ComonentTypeInNodeDic : IEnumerable<KeyValuePair<Type, HashSet<Type>>>
    {
        private Dictionary<Type, HashSet<Type>> componentTypeOfNode = new Dictionary<Type, HashSet<Type>>();

        public static ComonentTypeInNodeDic GetInstanceByAssembly()
        {
            ComonentTypeInNodeDic set = new ComonentTypeInNodeDic();
            set.componentTypeOfNode.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                set.addDataByAssembly(assembly);
            }

            return set;
        }

        public HashSet<Type> this[Type key]
        {
            get => componentTypeOfNode[key];
        }

        public static ComonentTypeInNodeDic operator +(ComonentTypeInNodeDic c1, ComonentTypeInNodeDic c2)
        {
            if (c1 == null)
            {
                return c2;
            }

            foreach (KeyValuePair<Type, HashSet<Type>> typePair in c2.componentTypeOfNode)
            {
                if (!c1.componentTypeOfNode.ContainsKey(typePair.Key))
                {
                    c1.componentTypeOfNode.Add(typePair.Key, typePair.Value);
                }
            }

            return c1;
        }

        private void addDataByAssembly(Assembly fromAssembly)
        {
            foreach (Type type in fromAssembly.GetTypes()
                .Where(myType =>
                {
                    return myType.IsClass
                        && !myType.IsAbstract
                        && myType.IsSubclassOf(typeof(EcsNode));
                }))
            {
                componentTypeOfNode.Add(type, new HashSet<Type>());

                IEnumerable<PropertyInfo> componentsField = type.GetProperties()
                    .Where((comType) =>
                            comType.PropertyType.IsClass
                            && !comType.PropertyType.IsAbstract
                            && comType.PropertyType.IsSubclassOf(typeof(EcsComponent)));
                foreach (PropertyInfo field in componentsField)
                {
                    if (field.GetCustomAttribute<OptionalComponentAttribute>() == null)
                    {
                        componentTypeOfNode[type].Add(field.PropertyType);
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<Type, HashSet<Type>>> GetEnumerator()
        {
            return componentTypeOfNode.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}