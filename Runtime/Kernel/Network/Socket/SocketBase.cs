using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Morpheus.Network
{
    internal abstract class SocketBase : ISocket, IDisposable
    {
        protected internal int id;
        protected internal int version;

        protected Socket socket;
        protected EndPoint bindingEndPoint;
        protected readonly SocketAoHandler onSocketAoComplete; // NOTE: When ReceiveAsync & SendAsync successfully will not send events.
        protected readonly int maxPacketSize;

        // Receive Related
        protected readonly int receiveBufferSize;
        protected SocketAsyncEventArgs receiveEventArgs;
        protected readonly IResponseProducer responseProducer;
        protected readonly SocketResponseHandler onResponseComplete;

        // Send Related
        protected readonly int sendBufferSize;
        protected SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
        protected Timer heartbeatTimer;
        protected int sendTimeout;

        public int Id => id;

        protected SocketBase(int id, SocketHandlerConfig handlerConfig)
        {
            this.id = id;
            onSocketAoComplete = handlerConfig.onSocketAoCompleteHandler;
            responseProducer = handlerConfig.responseProducer;
            onResponseComplete = handlerConfig.onResponseCompleteHandler;
        }

        protected SocketBase(int id, SocketConnectionConfig connConfig, SocketHandlerConfig handlerConfig)
        {
            this.id = id;
            IPAddress.TryParse(connConfig.ipAdderss, out IPAddress ipAddress);
            bindingEndPoint = new IPEndPoint(ipAddress, connConfig.port);
            onSocketAoComplete = handlerConfig.onSocketAoCompleteHandler;
            maxPacketSize = connConfig.maxPacketSize;

            receiveBufferSize = connConfig.maxPacketSize * 4;
            CreateReceiveEventArgs();
            responseProducer = handlerConfig.responseProducer;
            onResponseComplete = handlerConfig.onResponseCompleteHandler;

            sendBufferSize = connConfig.maxPacketSize * 2;
            CreateSendEventArgs();
            heartbeatTimer = new Timer(OnHeartbeatTimeout);
            sendTimeout = connConfig.sendTimeout;
        }

        protected void CreateReceiveEventArgs()
        {
            PacketBuffer packetBuf = new PacketBuffer
            {
                final = new byte[receiveBufferSize],
                compress = new byte[receiveBufferSize],
                encrypt = new byte[receiveBufferSize]
            };
            PacketReadState readState = new PacketReadState
            {
                packetBuf = packetBuf
            };
            receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.SetBuffer(packetBuf.final, 0, receiveBufferSize);
            receiveEventArgs.UserToken = readState;
            receiveEventArgs.Completed += OnReceiveAsyncComplete;
        }

        protected void CreateSendEventArgs()
        {
            PacketBuffer packetBuf = new PacketBuffer
            {
                final = new byte[short.MaxValue],
                compress = new byte[sendBufferSize],
                encrypt = new byte[sendBufferSize]
            };
            PacketSendState sendState = new PacketSendState
            {
                sendBuf = new byte[short.MaxValue],
                packetBuf = packetBuf
            };
            sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.SetBuffer(sendState.sendBuf, 0, short.MaxValue);
            sendEventArgs.UserToken = sendState;
            sendEventArgs.Completed += OnSendAsyncComplete;
        }

        // NOTE: Dispose and Reset called uniformly through the main thread.
        public abstract void Dispose();
        public abstract void Reset();

        public abstract void ConnectAsync();
        public abstract void DisconnectAsync();
        protected abstract void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs e);

        public abstract void ReceiveAsync();
        protected abstract void OnReceiveAsyncComplete(object sender, SocketAsyncEventArgs e);

        public abstract void SendAsync(IRequest request);
        protected abstract void OnSendAsyncComplete(object sender, SocketAsyncEventArgs e);

        private void OnHeartbeatTimeout(object state)
        {
            // TODO: Send heartbeat packet.
        }

        private void OnSendAsyncTimeout(object state)
        {
            onSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.TimedOut);
        }
    }
}