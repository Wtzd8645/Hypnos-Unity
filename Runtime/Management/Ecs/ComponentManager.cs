using System;
using System.Collections.Generic;

namespace Blanketmen.Hypnos
{
    public partial class ComponentManager : Singleton<ComponentManager>
    {
        public delegate void onComponentChange(EcsComponent component);
        public event onComponentChange OnComponentAdd;
        public event onComponentChange OnComponentRemove;

        private Dictionary<ulong, Dictionary<Type, EcsComponent>> entityToComponentsDict = new Dictionary<ulong, Dictionary<Type, EcsComponent>>();

        public T GetComponent<T>(ulong entityID) where T : EcsComponent
        {
            return !entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> componentDict)
                || !componentDict.TryGetValue(typeof(T), out EcsComponent result)
                ? null
                : result as T;
        }

        public T AddComponent<T>(ulong entityID) where T : EcsComponent
        {
            T component = GetComponent<T>();
            component.Init(entityID);
            if (entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> components))
            {
                components.Add(component.GetType(), component);
            }
            else
            {
                entityToComponentsDict[entityID] = new Dictionary<Type, EcsComponent>()
                {
                    {component.GetType(), component}
                };
            }

            OnComponentAdd(component);

            return component;
        }

        public void RemoveComponent<T>(ulong entityID) where T : EcsComponent
        {
            T component = null;
            if (entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> components))
            {
                component = (T)components[typeof(T)];
                components.Remove(typeof(T));
            }
            OnComponentRemove(component);

            component.Dispose();
            RecycleComponent(component);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityID);
            }
        }
    }
}