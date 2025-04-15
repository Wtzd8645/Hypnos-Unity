using System;

namespace Blanketmen.Hypnos.Network
{
    public interface IRequest
    {
        public ushort Id { get; }

        public unsafe ushort Pack(Span<byte> buf);
    }
}