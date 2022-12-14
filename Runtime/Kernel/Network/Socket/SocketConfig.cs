using System;

namespace Morpheus.Network
{
    [Serializable]
    public class SocketConnectionConfig
    {
        public TransportProtocol Protocol;
        public string IpAdderss;
        public int Port = NetworkManager.DefalutPort;

        public ushort MaxPacketSize = NetworkManager.DefalutMaxBufferSize;
        public int SendTimeout = NetworkManager.DefalutSendTimeout;
    }

    internal class SocketHandlerConfig
    {
        public SocketAoHandler OnSocketAoCompleteHandler;
        public IResponseProducer ResponseProducer;
        public SocketResponseHandler OnResponseCompleteHandler;
    }

    [Serializable]
    public class SocketConfig
    {
        public int Id;
        public SocketConnectionConfig ConnectionConfig;
        public int ResponseProducerId;
    }
}