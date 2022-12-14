using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.Ecs
{
    public partial class ComponentManager : Singleton<ComponentManager>
    {
        public event Action<EcsComponent> OnComponentAdd;
        public event Action<EcsComponent> OnComponentRemove;

        private Dictionary<ulong, Dictionary<Type, EcsComponent>> entityToComponentsDict = new Dictionary<ulong, Dictionary<Type, EcsComponent>>();

        private NodeManager nodeManager;
        
        public ComponentManager()
        {
            nodeManager = new NodeManager();
            OnComponentAdd += nodeManager.OnComponentAdd;
            OnComponentRemove += nodeManager.OnComponentRemove;
        }

        public override void Dispose()
        {
            base.Dispose();
            OnComponentAdd -= nodeManager.OnComponentAdd;
            OnComponentRemove -= nodeManager.OnComponentRemove;
        }

        public T GetComponent<T>(ulong entityId) where T : EcsComponent
        {
            return !entityToComponentsDict.TryGetValue(entityId, out Dictionary<Type, EcsComponent> componentDict)
                || !componentDict.TryGetValue(typeof(T), out EcsComponent result)
                ? null
                : result as T;
        }

        internal IEnumerable<EcsComponent> GetAllComponents(ulong entityId)
        {
            if (!entityToComponentsDict.TryGetValue(entityId, out Dictionary<Type, EcsComponent> componentDict))
            {
                return null;
            }

            return componentDict.Values.ToArray();
        }

        public void AddComponent<T>(ulong entityId,
            IComponentConfig config = null,
            Action<T> onComplete = null) where T : EcsComponent
        {
            T component = GetComponent<T>();
            if (entityToComponentsDict.TryGetValue(entityId, out Dictionary<Type, EcsComponent> components))
            {
                components.Add(component.GetType(), component);
            }
            else
            {
                entityToComponentsDict[entityId] = new Dictionary<Type, EcsComponent>()
                {
                    {component.GetType(), component}
                };
            }
            
            config?.Apply(component);
            component.Use(
                entityId,
                ()=>
                {
                    Logger.TraceLog($"Entity {entityId} Added Component {typeof(T)}");

                    OnComponentAdd.Invoke(component);
                    onComplete?.Invoke(component);
                });
        }
        
        public void RemoveComponent(ulong entityId, EcsComponent c)
        {
            if (entityToComponentsDict.TryGetValue(entityId, out Dictionary<Type, EcsComponent> components))
            {
                components.Remove(c.GetType());
            }

            Logger.TraceLog($"Entity {entityId} Removed Component {c.GetType()}");

            OnComponentRemove.Invoke(c);

            c.Dispose();
            RecycleComponent(c);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityId);
            }
        }

        public void RemoveComponent<T>(ulong entityId) where T : EcsComponent
        {
            T component = null;
            if (entityToComponentsDict.TryGetValue(entityId, out Dictionary<Type, EcsComponent> components))
            {
                component = (T)components[typeof(T)];
                components.Remove(typeof(T));
            }

            Logger.TraceLog($"Entity {entityId} Remove Component {typeof(T)}");

            OnComponentRemove.Invoke(component);

            component.Dispose();
            RecycleComponent(component);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityId);
            }
        }
    }
}