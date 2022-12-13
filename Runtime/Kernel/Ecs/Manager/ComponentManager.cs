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

        public T GetComponent<T>(ulong entityID) where T : EcsComponent
        {
            return !entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> componentDict)
                || !componentDict.TryGetValue(typeof(T), out EcsComponent result)
                ? null
                : result as T;
        }

        internal IEnumerable<EcsComponent> GetAllComponents(ulong entityID)
        {
            if (!entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> componentDict))
            {
                return null;
            }

            return componentDict.Values.ToArray();
        }

        public void AddComponent<T>(ulong entityID,
            IComponentConfig config = null,
            Action<T> onComplete = null) where T : EcsComponent
        {
            T component = getComponent<T>();
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
            
            config?.Apply(component);
            component.Use(
                entityID,
                ()=>
                {
                    DebugLogger.TraceLog($"Entity {entityID} Added Component {typeof(T)}");

                    OnComponentAdd.Invoke(component);
                    onComplete?.Invoke(component);
                });
        }
        
        public void RemoveComponent(ulong entityID, EcsComponent c)
        {
            if (entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> components))
            {
                components.Remove(c.GetType());
            }

            DebugLogger.TraceLog($"Entity {entityID} Removed Component {c.GetType()}");

            OnComponentRemove.Invoke(c);

            c.Dispose();
            recycleComponent(c);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityID);
            }
        }

        public void RemoveComponent<T>(ulong entityID) where T : EcsComponent
        {
            T component = null;
            if (entityToComponentsDict.TryGetValue(entityID, out Dictionary<Type, EcsComponent> components))
            {
                component = (T)components[typeof(T)];
                components.Remove(typeof(T));
            }

            DebugLogger.TraceLog($"Entity {entityID} Remove Component {typeof(T)}");

            OnComponentRemove.Invoke(component);

            component.Dispose();
            recycleComponent(component);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityID);
            }
        }
    }
}