using Blanketmen.Hypnos.Cache;
using System;
using System.Collections.Generic;

namespace Blanketmen.Hypnos
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
            return (T)objectPool.Pop();
        }

        private void RecycleComponent<T>(T component) where T : EcsComponent
        {
            Type type = typeof(T);
            if (componentPools.TryGetValue(type, out ObjectPool<EcsComponent> objectPool))
            {
                objectPool.Push(component);
            }
            else
            {
                throw new Exception($"Can't find Component object pool of Type {type}.");
            }
        }
    }
}