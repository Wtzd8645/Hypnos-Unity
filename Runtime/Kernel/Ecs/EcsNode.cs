using System;

namespace Hypnos.Ecs
{
    public class EcsNode : IDisposable
    {
        public EcsNode LastNode = null;
        public EcsNode NextNode = null;

        private ulong holderEntity = 0;

        public void Init(ulong entity)
        {
            holderEntity = entity;
        }

        protected T GetComponent<T>() where T : EcsComponent
        {
            return ComponentManager.Instance.GetComponent<T>(holderEntity);
        }

        public virtual void Dispose()
        {
            LastNode = null;
            NextNode = null;
            holderEntity = 0;
        }
    }
}