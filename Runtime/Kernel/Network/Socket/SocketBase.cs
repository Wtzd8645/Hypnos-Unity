using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Morpheus.Network
{
    internal abstract class SocketBase : ISocket, IDisposable
    {
        protected internal int SocketId;
        protected internal int Version;

        protected Socket Socket;
        protected EndPoint BindingEndPoint;
        protected readonly SocketAoHandler OnSocketAoComplete; // NOTE: When ReceiveAsync & SendAsync successfully will not send events.
        protected readonly int MaxPacketSize;

        // Receive Related
        protected readonly int ReceiveBufferSize;
        protected SocketAsyncEventArgs ReceiveEventArgs;
        protected readonly IResponseProducer ResponseProducer;
        protected readonly SocketResponseHandler OnResponseComplete;

        // Send Related
        protected readonly int SendBufferSize;
        protected SocketAsyncEventArgs SendEventArgs = new SocketAsyncEventArgs();
        protected Timer HeartbeatTimer;
        protected int SendTimeout;

        public int Id => SocketId;

        protected SocketBase(int _id, SocketHandlerConfig handlerConfig)
        {
            SocketId = _id;
            OnSocketAoComplete = handlerConfig.OnSocketAoCompleteHandler;
            ResponseProducer = handlerConfig.ResponseProducer;
            OnResponseComplete = handlerConfig.OnResponseCompleteHandler;
        }

        protected SocketBase(int _id, SocketConnectionConfig connConfig, SocketHandlerConfig handlerConfig)
        {
            SocketId = _id;
            IPAddress.TryParse(connConfig.IpAdderss, out IPAddress ipAddress);
            BindingEndPoint = new IPEndPoint(ipAddress, connConfig.Port);
            OnSocketAoComplete = handlerConfig.OnSocketAoCompleteHandler;
            MaxPacketSize = connConfig.MaxPacketSize;

            ReceiveBufferSize = connConfig.MaxPacketSize * 4;
            CreateReceiveEventArgs();
            ResponseProducer = handlerConfig.ResponseProducer;
            OnResponseComplete = handlerConfig.OnResponseCompleteHandler;

            SendBufferSize = connConfig.MaxPacketSize * 2;
            CreateSendEventArgs();
            HeartbeatTimer = new Timer(OnHeartbeatTimeout);
            SendTimeout = connConfig.SendTimeout;
        }

        protected void CreateReceiveEventArgs()
        {
            PacketBuffer packetBuf = new PacketBuffer
            {
                Final = new byte[ReceiveBufferSize],
                Compress = new byte[ReceiveBufferSize],
                Encrypt = new byte[ReceiveBufferSize]
            };
            PacketReadState readState = new PacketReadState
            {
                PacketBuf = packetBuf
            };
            ReceiveEventArgs = new SocketAsyncEventArgs();
            ReceiveEventArgs.SetBuffer(packetBuf.Final, 0, ReceiveBufferSize);
            ReceiveEventArgs.UserToken = readState;
            ReceiveEventArgs.Completed += OnReceiveAsyncComplete;
        }

        protected void CreateSendEventArgs()
        {
            PacketBuffer packetBuf = new PacketBuffer
            {
                Final = new byte[short.MaxValue],
                Compress = new byte[SendBufferSize],
                Encrypt = new byte[SendBufferSize]
            };
            PacketSendState sendState = new PacketSendState
            {
                SendBuf = new byte[short.MaxValue],
                PacketBuf = packetBuf
            };
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.SetBuffer(sendState.SendBuf, 0, short.MaxValue);
            SendEventArgs.UserToken = sendState;
            SendEventArgs.Completed += OnSendAsyncComplete;
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
            OnSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.TimedOut);
        }
    }
}