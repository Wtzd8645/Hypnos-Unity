using System;

namespace Morpheus.Ecs
{
    public abstract class EcsComponent : IDisposable
    {
        public ulong EntityID { get; private set; }

        ///<summary>
        /// Remember to put the 'base.Use(...)' on the last line in your impl
        ///</summary>
        public virtual void Use(ulong id, Action onComplete = null)
        {
            EntityID = id;
            onComplete?.Invoke();
        }

        public virtual void Dispose()
        {
            EntityID = 0;
        }

        //TODO: Be careful
        public override int GetHashCode()
        {
            return (int)EntityID;
        }

        public override string ToString() => $"[{EntityID}]{GetType().ToString()}";
    }
}