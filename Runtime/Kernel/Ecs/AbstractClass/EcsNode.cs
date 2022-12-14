using System;

namespace Morpheus.Ecs
{
    public abstract class EcsNode : IDisposable
    {
        public EcsNode PrevNode = null;
        public EcsNode NextNode = null;

        public ulong HolderEntity {get; private set;} = 0;

        public virtual void Init(ulong entity)
        {
            HolderEntity = entity;
        }

        protected T GetComponent<T>() where T : EcsComponent
        {
            return ComponentManager.Instance.GetComponent<T>(HolderEntity);
        }

        public virtual void Dispose()
        {
            PrevNode = null;
            NextNode = null;
            HolderEntity = 0;
        }
    }
}