using System;

namespace Morpheus.Network
{
    [Serializable]
    public class SocketConfig
    {
        public TransportProtocol protocol;
        public string ipAdderss;
        public int port = NetworkManager.DefalutPort;

        public ushort maxPacketSize = NetworkManager.DefalutMaxPacketSize;
        public int sendTimeout = NetworkManager.DefalutSendTimeout;
    }

    internal class ConnectionHandlerConfig
    {
        public ConnectionAoHandler onConnectionAoCompleteHandler;
        public IResponseProducer responseProducer;
    }

    [Serializable]
    public class ConnectionConfig
    {
        public int id;
        public SocketConfig socketConfig;
        public int responseProducerId;
    }
}