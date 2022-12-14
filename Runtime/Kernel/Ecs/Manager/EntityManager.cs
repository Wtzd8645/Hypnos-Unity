using System;
using System.Reflection;
using System.Collections.Generic;

namespace Morpheus.Ecs
{
    //TODO Performance analyze
    //TODO: 平行化
    public class EntityManager : Singleton<EntityManager>
    {
        public event Action<ulong> OnEntityAdd = (id)=>{
            Logger.TraceLog($"Entity {id} spawned.");
        };
        public event Action<ulong> OnEntityRemove = (id)=>{
            Logger.TraceLog($"Entity {id} removed.");
        };

        private MethodInfo addComponentInfo;
        private Delegate componentAddedDelegate;

        private ulong nowEntityId = 0;
        private IEntityConfig nowConfig = null;
        private Action<ulong> onComplete = null;
        private IEnumerator<IComponentConfig> configEnumerator = null;
        
        public EntityManager()
        {
            addComponentInfo = typeof(ComponentManager).GetMethod("AddComponent");

            var mi1 = typeof(EntityManager).GetMethod("OnComponentAdded",
                BindingFlags.NonPublic | BindingFlags.Instance);
            componentAddedDelegate = Delegate.CreateDelegate(typeof(Action<EcsComponent>), this, mi1, false);
        }

        private void OnComponentAdded(EcsComponent c)
        {
            SpawnEntity();
        }

        private void SpawnEntity()
        {
            if(configEnumerator.MoveNext())
            {
                var gm = addComponentInfo.MakeGenericMethod(configEnumerator.Current.Type);
                gm.Invoke(
                    ComponentManager.Instance,
                    new object[] {nowEntityId, configEnumerator.Current, componentAddedDelegate});
            }
            else
            {
                OnEntityAdd?.Invoke(nowEntityId);
                onComplete?.Invoke(nowEntityId);
            }
        }

        public void SpawnEntity(IEntityConfig _config, Action<ulong> _onComplete = null)
        {
            nowEntityId = IdGenerator.Get();
            nowConfig = _config;
            onComplete = _onComplete;
            configEnumerator = _config.ComponentTypes.GetEnumerator();
            SpawnEntity();
        }

        public void DestroyEntity(ulong entityId)
        {
            var allComponent = ComponentManager.Instance.GetAllComponents(entityId);
            foreach(var c in allComponent)
            {
                ComponentManager.Instance.RemoveComponent(entityId, c);
            }
            OnEntityRemove.Invoke(entityId);
        }
    }
}