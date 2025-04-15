namespace Blanketmen.Hypnos.Network
{
    internal interface IClientSocket : ISocket
    {
        public void SetAddress(string ip, int port);

        public void Send(IRequest req);

        public void Register(ClientEventHandler handler);
        public void Unregister(ClientEventHandler handler);

        public void Register(ushort gid, ResponseHandler handler);
        public void Unregister(ushort gid, ResponseHandler handler);
    }
}