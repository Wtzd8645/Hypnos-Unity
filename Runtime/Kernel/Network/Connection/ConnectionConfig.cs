using System;

namespace Morpheus.Network
{
    [Serializable]
    public class TransportConfig
    {
        public TransportProtocol protocol;
        public string ip;
        public int port = NetworkManager.DefalutPort;

        public ushort maxPacketSize = NetworkManager.DefalutMaxPacketSize;
        public int sendTimeout = NetworkManager.DefalutSendTimeout;
    }

    public class HandlerConfig
    {
        public ConnectionAoHandler onConnectionAoCompleteHandler;
        public IResponseProducer responseProducer;
    }

    [Serializable]
    public class ConnectionConfig
    {
        public int id;
        public TransportConfig transportConfig;
        public int responseProducerId;
    }
}