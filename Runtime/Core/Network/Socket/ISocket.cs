namespace Blanketmen.Hypnos.Network
{
    public interface ISocket
    {
        public uint Id { get; }
        public uint Version { get; }

        public void Dispose();
        public void Reset();
        public void ConnectAsync();
        public void DisconnectAsync();
        public void ReceiveAsync();
        public bool TryGetResponse(out IResponse response);
        public void SendAsync(IRequest request);
    }
}