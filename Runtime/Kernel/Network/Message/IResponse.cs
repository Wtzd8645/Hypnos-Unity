namespace Morpheus.Network
{
    public interface IResponse
    {
        public ushort Id { get; set; }

        public void Unpack(PacketBuffer source);
    }
}