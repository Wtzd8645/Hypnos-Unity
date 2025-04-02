using System;

namespace Blanketmen.Hypnos.Network
{
    [Serializable]
    public class TransportConfig
    {
        public TransportProtocol protocol;
        public string ip;
        public int port = 27015;

        public ushort maxPacketSize = NetworkDefs.MAX_PACKET_SIZE;
        public int sendTimeout = NetworkDefs.DEFAULT_SEND_TIMEOUT;
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