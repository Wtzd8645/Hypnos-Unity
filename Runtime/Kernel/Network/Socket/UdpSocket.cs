using System;
using System.Net.Sockets;

namespace Morpheus.Network
{
    internal class UdpSocket : SocketBase
    {
        public UdpSocket(int id, SocketConnectionConfig connConfig, SocketHandlerConfig handlerConfig) : base(id, connConfig, handlerConfig)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ReceiveEventArgs.RemoteEndPoint = BindingEndPoint;
            SendEventArgs.RemoteEndPoint = BindingEndPoint;
        }

        private void CreateSocket()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ++Version;
        }

        public override void Dispose()
        {
            Socket.Close();
        }

        public override void Reset()
        {
            Kernel.Log($"[UdpSocket] Reset. Id: {Id}", (int)LogChannel.Network);
            Socket.Close();
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            ReceiveEventArgs.RemoteEndPoint = BindingEndPoint;
            SendEventArgs.RemoteEndPoint = BindingEndPoint;
        }

        public override void ConnectAsync()
        {
            OnSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.Success);
        }

        public override void DisconnectAsync()
        {
            OnSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.Success);
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs evtArgs) { }

        public override void ReceiveAsync()
        {
            Kernel.Log($"[UdpSocket] ReceiveAsync. Id: {Id}", (int)LogChannel.Network);
            ReceiveEventArgs.SetBuffer(0, ReceiveBufferSize);
            ReceiveInternalAsync(Socket, ReceiveEventArgs);
        }

        private void ReceiveInternalAsync(Socket connSocket, SocketAsyncEventArgs evtArgs)
        {
            try
            {
                if (!connSocket.ReceiveFromAsync(evtArgs))
                {
                    OnReceiveAsyncComplete(null, evtArgs);
                }
            }
            catch (Exception e)
            {
                Kernel.LogError($"[UdpSocket] ReceiveAsync failed. Id: {Id}, Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Receive, evtArgs.SocketError);
            }
        }

        protected override void OnReceiveAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            if (evtArgs.SocketError != SocketError.Success) // Abnormal shutdown.
            {
                OnSocketAoComplete(this, SocketAsyncOperation.Receive, evtArgs.SocketError);
                return;
            }

            if (evtArgs.BytesTransferred == 0) // Normal shutdown.
            {
                OnSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.Disconnecting);
                return;
            }

            Kernel.TraceLog($"[UdpSocket] Socket {Id} received {evtArgs.BytesTransferred} bytes", (int)LogChannel.Network);
            try
            {
                PacketReadState readState = evtArgs.UserToken as PacketReadState;
                readState.PacketBuf.Offset = NetworkManager.PacketLengthSize;
                OnResponseComplete(this, ResponseProducer.Produce(readState.PacketBuf));
            }
            catch (Exception e)
            {
                Kernel.LogError($"[UdpSocket] Socket {Id} create message failed. Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
            }

            evtArgs.SetBuffer(0, ReceiveBufferSize);
            ReceiveInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }

        public override void SendAsync(IRequest request)
        {
            PacketSendState sendState = SendEventArgs.UserToken as PacketSendState;
            lock (SendEventArgs)
            {
                if (sendState.IsSending)
                {
                    int packetBytes = request.Pack(sendState.PacketBuf);
                    int producedBytes = sendState.PacketBuf.Offset + packetBytes;
                    if (packetBytes > MaxPacketSize || producedBytes > sendState.PacketBuf.Final.Length)
                    {
                        OnSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                        return;
                    }

                    sendState.PacketBuf.Offset = producedBytes;
                    Kernel.TraceLog($"[UdpSocket] Socket {Id} produce {producedBytes} bytes.", (int)DebugLogChannel.Network);
                    return;
                }
            }

            sendState.IsSending = true;
            sendState.ProcessedBytes = 0;
            sendState.PendingBytes = request.Pack(sendState.PacketBuf);
            if (sendState.PendingBytes > MaxPacketSize)
            {
                OnSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                return;
            }

            Kernel.TraceLog($"[UdpSocket] Socket {Id} send {sendState.PendingBytes} bytes.", (int)LogChannel.Network);
            byte[] buffer = SendEventArgs.Buffer;
            SendEventArgs.SetBuffer(sendState.PacketBuf.Final, 0, sendState.PendingBytes);
            sendState.PacketBuf.Offset = 0;
            sendState.PacketBuf.Final = buffer;
            SendInternalAsync(Socket, SendEventArgs);
        }

        private void SendInternalAsync(Socket connSocket, SocketAsyncEventArgs evtArgs)
        {
            try
            {
                if (!connSocket.SendToAsync(evtArgs))
                {
                    OnSendAsyncComplete(null, evtArgs);
                }
            }
            catch (Exception e)
            {
                Kernel.LogError($"[UdpSocket] SendAsync failed. Id: {Id}, Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.SocketError);
            }
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            Kernel.TraceLog($"[UdpSocket] Socket {Id} sent {evtArgs.BytesTransferred} bytes.", (int)LogChannel.Network);
            PacketSendState sendState = evtArgs.UserToken as PacketSendState;
            if (evtArgs.SocketError != SocketError.Success)
            {
                OnSocketAoComplete(this, SocketAsyncOperation.Send, evtArgs.SocketError);
                return;
            }

            sendState.PendingBytes -= evtArgs.BytesTransferred;
            if (sendState.PendingBytes > 0)
            {
                sendState.ProcessedBytes += evtArgs.BytesTransferred;
                evtArgs.SetBuffer(sendState.ProcessedBytes, sendState.PendingBytes);
                SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
                return;
            }

            lock (SendEventArgs)
            {
                if (sendState.PacketBuf.Offset == 0)
                {
                    sendState.IsSending = false;
                    return;
                }

                Kernel.TraceLog($"[UdpSocket] Socket {Id} send {sendState.PacketBuf.Offset} produced bytes.", (int)LogChannel.Network);
                byte[] buffer = evtArgs.Buffer;
                evtArgs.SetBuffer(sendState.PacketBuf.Final, 0, sendState.PacketBuf.Offset);
                sendState.PendingBytes = sendState.PacketBuf.Offset;
                sendState.ProcessedBytes = 0;
                sendState.PacketBuf.Offset = 0;
                sendState.PacketBuf.Final = buffer;
            }
            SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }
    }
}