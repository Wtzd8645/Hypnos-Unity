namespace Morpheus.Network
{
    internal interface ISocket
    {
        public int Id { get; }
        public void Dispose();
        public void Reset();
        public void ConnectAsync();
        public void DisconnectAsync();
        public void ReceiveAsync();
        public void SendAsync(IRequest request);
    }
}