using System;

namespace Morpheus.Ecs
{
    public abstract class EcsComponent : IDisposable
    {
        public ulong EntityId { get; private set; }

        ///<summary>
        /// Remember to put the 'base.Use(...)' on the last line in your impl
        ///</summary>
        public virtual void Use(ulong id, Action onComplete = null)
        {
            EntityId = id;
            onComplete?.Invoke();
        }

        public virtual void Dispose()
        {
            EntityId = 0;
        }

        //TODO: Be careful
        public override int GetHashCode()
        {
            return (int)EntityId;
        }

        public override string ToString() => $"[{EntityId}]{GetType().ToString()}";
    }
}