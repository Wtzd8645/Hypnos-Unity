namespace Blanketmen.Hypnos.Network
{
    internal class MockClientSocket : IClientSocket
    {
        public MockClientSocket(SocketConfig cfg)
        {

        }

        public uint Id => throw new System.NotImplementedException();

        public uint Version => throw new System.NotImplementedException();
        
        public void SetAddress(string ip, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void Dispatch()
        {
            throw new System.NotImplementedException();
        }

        public void Send(IRequest req)
        {
            throw new System.NotImplementedException();
        }

        public void Register(ClientEventHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public void Unregister(ClientEventHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public void Register(ushort gid, ResponseHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public void Unregister(ushort gid, ResponseHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public void Process(IOEventArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
}