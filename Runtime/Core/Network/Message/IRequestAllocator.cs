using System;

namespace Blanketmen.Hypnos.Network
{
    public interface IRequestAllocator
    {
        public IRequest Acquire(ushort gid, ushort id);
        public IRequest Acquire(Span<byte> buf);
        public void Release(IRequest req);
    }
}