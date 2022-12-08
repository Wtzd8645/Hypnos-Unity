using System;

namespace Morpheus.Network
{
    [Serializable]
    public class SocketConnectionConfig
    {
        public TransportProtocol protocol;
        public string ipAdderss;
        public int port = NetworkManager.DefalutPort;

        public ushort maxPacketSize = NetworkManager.DefalutMaxBufferSize;
        public int sendTimeout = NetworkManager.DefalutSendTimeout;
    }

    internal class SocketHandlerConfig
    {
        public SocketAoHandler onSocketAoCompleteHandler;
        public IResponseProducer responseProducer;
        public SocketResponseHandler onResponseCompleteHandler;
    }

    [Serializable]
    public class SocketConfig
    {
        public int id;
        public SocketConnectionConfig connectionConfig;
        public int responseProducerId;
    }
}