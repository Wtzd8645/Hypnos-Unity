using System;

namespace Blanketmen.Hypnos.Network
{
    public interface IResponseAllocator
    {
        public IResponse Acquire(byte gid, ushort id);
        public IResponse Acquire(Span<byte> buf);
        public void Release(IResponse resp);
    }
}