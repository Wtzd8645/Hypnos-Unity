using Morpheus.Core.Memory;
using System;
using System.Collections.Generic;

namespace Morpheus.Ecs
{
    public partial class ComponentManager : Singleton<ComponentManager>
    {
        private Dictionary<Type, ObjectPool<EcsComponent>> componentPools = new Dictionary<Type, ObjectPool<EcsComponent>>();

        private T GetComponent<T>() where T : EcsComponent
        {
            Type type = typeof(T);
            if (!componentPools.TryGetValue(type, out ObjectPool<EcsComponent> objectPool))
            {
                objectPool = new ObjectPool<EcsComponent>(() => (EcsComponent)Activator.CreateInstance(type));
                componentPools[type] = objectPool;
            }
            return (T)objectPool.ForcePop();
        }

        private void RecycleComponent<T>(T component) where T : EcsComponent
        {
            Type type = typeof(T);
            if (componentPools.TryGetValue(type, out ObjectPool<EcsComponent> objectPool))
            {
                objectPool.ForcePush(component);
            }
            else
            {
                throw new Exception($"Can't find Component object pool of Type {type}.");
            }
        }
    }
}