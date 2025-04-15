using System;

namespace Blanketmen.Hypnos.Network
{
    public interface IResponse
    {
        public byte Gid { get; set; }
        public ushort Id { get; set; }

        public void Unpack(Span<byte> buf);
    }
}