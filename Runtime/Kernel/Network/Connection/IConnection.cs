namespace Blanketmen.Hypnos
{
    public interface IConnection
    {
        public int Id { get; }
        public int Version { get; }

        public void Dispose();
        public void Reset();
        public void ConnectAsync();
        public void DisconnectAsync();
        public void ReceiveAsync();
        public bool TryGetResponse(out IResponse response);
        public void SendAsync(IRequest request);
    }
}