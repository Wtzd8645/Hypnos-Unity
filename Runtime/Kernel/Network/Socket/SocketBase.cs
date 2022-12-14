using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Morpheus.Network
{
    internal abstract class SocketBase : IConnection, IDisposable
    {
        protected int id;
        protected int version;

        protected Socket socket;
        protected EndPoint bindingEndPoint;
        protected readonly ConnectionAoHandler onConnectionAoComplete; // NOTE: When ReceiveAsync & SendAsync successfully will not send events.
        protected readonly int maxPacketSize;

        // Receive Related
        protected int receiveBufferSize;
        protected SocketAsyncEventArgs receiveEventArgs;
        protected readonly IResponseProducer responseProducer;
        protected readonly ConcurrentQueue<IResponse> pendingResponses;

        // Send Related
        protected SocketAsyncEventArgs sendEventArgs;
        protected Timer heartbeatTimer;
        protected int sendTimeout;

        public int Id => id;
        public int Version => version;

        protected SocketBase(int id, ConnectionHandlerConfig handlerConfig)
        {
            this.id = id;
            onConnectionAoComplete = handlerConfig.onConnectionAoCompleteHandler;
            responseProducer = handlerConfig.responseProducer;
        }

        protected SocketBase(int id, SocketConfig socketConfig, ConnectionHandlerConfig handlerConfig)
        {
            this.id = id;
            IPAddress.TryParse(socketConfig.ipAdderss, out IPAddress ipAddress);
            bindingEndPoint = new IPEndPoint(ipAddress, socketConfig.port);
            onConnectionAoComplete = handlerConfig.onConnectionAoCompleteHandler;
            maxPacketSize = socketConfig.maxPacketSize;

            CreateReceiveEventArgs();
            responseProducer = handlerConfig.responseProducer;
            pendingResponses = new ConcurrentQueue<IResponse>();

            CreateSendEventArgs();
            heartbeatTimer = new Timer(OnHeartbeatTimeout);
            sendTimeout = socketConfig.sendTimeout;
        }

        protected void CreateReceiveEventArgs()
        {
            receiveBufferSize = maxPacketSize * 4;
            PacketBuffer packetBuf = new PacketBuffer
            {
                final = new byte[receiveBufferSize],
                compress = new byte[maxPacketSize],
                encrypt = new byte[maxPacketSize]
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
            int sendBufSize = maxPacketSize * 16;
            PacketBuffer sendBuf = new PacketBuffer
            {
                final = new byte[sendBufSize],
                compress = new byte[maxPacketSize],
                encrypt = new byte[maxPacketSize]
            };
            PacketBuffer packetBuf = new PacketBuffer
            {
                final = new byte[sendBufSize],
                compress = sendBuf.compress,
                encrypt = sendBuf.encrypt
            };
            PacketSendState sendState = new PacketSendState
            {
                sendBuf = sendBuf,
                packetBuf = packetBuf
            };
            sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.SetBuffer(sendBuf.final, 0, sendBufSize);
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

        public bool TryGetResponse(out IResponse response)
        {
            return pendingResponses.TryDequeue(out response);
        }

        protected abstract void OnReceiveAsyncComplete(object sender, SocketAsyncEventArgs e);

        public abstract void SendAsync(IRequest request);

        protected abstract void OnSendAsyncComplete(object sender, SocketAsyncEventArgs e);

        private void OnHeartbeatTimeout(object state)
        {
            // TODO: Send heartbeat packet.
        }

        private void OnSendAsyncTimeout(object state)
        {
            onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.TimedOut);
        }
    }
}