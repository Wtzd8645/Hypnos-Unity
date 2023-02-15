using System;

namespace Hypnos.Ecs
{
    public class EcsComponent : IDisposable
    {
        public ulong EntityID { get; private set; }

        public virtual void Init(ulong id)
        {
            EntityID = id;
        }

        public virtual void Dispose()
        {
            EntityID = 0;
        }

        public override string ToString() => GetType().ToString();
    }
}