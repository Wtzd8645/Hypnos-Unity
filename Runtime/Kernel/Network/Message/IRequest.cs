namespace Hypnos.Network
{
    public interface IRequest
    {
        public ushort Id { get; }

        public unsafe int Pack(PacketBuffer result);
    }
}