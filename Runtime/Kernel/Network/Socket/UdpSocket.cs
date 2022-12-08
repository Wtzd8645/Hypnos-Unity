using System;
using System.Net.Sockets;

namespace Morpheus.Network
{
    internal class UdpSocket : SocketBase
    {
        public UdpSocket(int id, SocketConnectionConfig connConfig, SocketHandlerConfig handlerConfig) : base(id, connConfig, handlerConfig)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiveEventArgs.RemoteEndPoint = bindingEndPoint;
            sendEventArgs.RemoteEndPoint = bindingEndPoint;
        }

        private void CreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ++version;
        }

        public override void Dispose()
        {
            socket.Close();
        }

        public override void Reset()
        {
            DebugLogger.Log($"[UdpSocket] Reset. Id: {id}", (int)DebugLogChannel.Network);
            socket.Close();
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            receiveEventArgs.RemoteEndPoint = bindingEndPoint;
            sendEventArgs.RemoteEndPoint = bindingEndPoint;
        }

        public override void ConnectAsync()
        {
            onSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.Success);
        }

        public override void DisconnectAsync()
        {
            onSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.Success);
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs evtArgs) { }

        public override void ReceiveAsync()
        {
            DebugLogger.Log($"[UdpSocket] ReceiveAsync. Id: {id}", (int)DebugLogChannel.Network);
            receiveEventArgs.SetBuffer(0, receiveBufferSize);
            ReceiveInternalAsync(socket, receiveEventArgs);
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
                DebugLogger.LogError($"[UdpSocket] ReceiveAsync failed. Id: {id}, Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Receive, evtArgs.SocketError);
            }
        }

        protected override void OnReceiveAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            if (evtArgs.SocketError != SocketError.Success) // Abnormal shutdown.
            {
                onSocketAoComplete(this, SocketAsyncOperation.Receive, evtArgs.SocketError);
                return;
            }

            if (evtArgs.BytesTransferred == 0) // Normal shutdown.
            {
                onSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.Disconnecting);
                return;
            }

            DebugLogger.TraceLog($"[UdpSocket] Socket {id} received {evtArgs.BytesTransferred} bytes", (int)DebugLogChannel.Network);
            try
            {
                PacketReadState readState = evtArgs.UserToken as PacketReadState;
                readState.packetBuf.offset = NetworkManager.PacketLengthSize;
                onResponseComplete(this, responseProducer.Produce(readState.packetBuf));
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[UdpSocket] Socket {id} create message failed. Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
            }

            evtArgs.SetBuffer(0, receiveBufferSize);
            ReceiveInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }

        public override void SendAsync(IRequest request)
        {
            PacketSendState sendState = sendEventArgs.UserToken as PacketSendState;
            lock (sendEventArgs)
            {
                if (sendState.isSending)
                {
                    int packetBytes = request.Pack(sendState.packetBuf);
                    int producedBytes = sendState.packetBuf.offset + packetBytes;
                    if (packetBytes > maxPacketSize || producedBytes > sendState.packetBuf.final.Length)
                    {
                        onSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                        return;
                    }

                    sendState.packetBuf.offset = producedBytes;
                    DebugLogger.TraceLog($"[UdpSocket] Socket {id} produce {producedBytes} bytes.", (int)DebugLogChannel.Network);
                    return;
                }
            }

            sendState.isSending = true;
            sendState.processedBytes = 0;
            sendState.pendingBytes = request.Pack(sendState.packetBuf);
            if (sendState.pendingBytes > maxPacketSize)
            {
                onSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                return;
            }

            DebugLogger.TraceLog($"[UdpSocket] Socket {id} send {sendState.pendingBytes} bytes.", (int)DebugLogChannel.Network);
            byte[] buffer = sendEventArgs.Buffer;
            sendEventArgs.SetBuffer(sendState.packetBuf.final, 0, sendState.pendingBytes);
            sendState.packetBuf.offset = 0;
            sendState.packetBuf.final = buffer;
            SendInternalAsync(socket, sendEventArgs);
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
                DebugLogger.LogError($"[UdpSocket] SendAsync failed. Id: {id}, Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.SocketError);
            }
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            DebugLogger.TraceLog($"[UdpSocket] Socket {id} sent {evtArgs.BytesTransferred} bytes.", (int)DebugLogChannel.Network);
            PacketSendState sendState = evtArgs.UserToken as PacketSendState;
            if (evtArgs.SocketError != SocketError.Success)
            {
                onSocketAoComplete(this, SocketAsyncOperation.Send, evtArgs.SocketError);
                return;
            }

            sendState.pendingBytes -= evtArgs.BytesTransferred;
            if (sendState.pendingBytes > 0)
            {
                sendState.processedBytes += evtArgs.BytesTransferred;
                evtArgs.SetBuffer(sendState.processedBytes, sendState.pendingBytes);
                SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
                return;
            }

            lock (sendEventArgs)
            {
                if (sendState.packetBuf.offset == 0)
                {
                    sendState.isSending = false;
                    return;
                }

                DebugLogger.TraceLog($"[UdpSocket] Socket {id} send {sendState.packetBuf.offset} produced bytes.", (int)DebugLogChannel.Network);
                byte[] buffer = evtArgs.Buffer;
                evtArgs.SetBuffer(sendState.packetBuf.final, 0, sendState.packetBuf.offset);
                sendState.pendingBytes = sendState.packetBuf.offset;
                sendState.processedBytes = 0;
                sendState.packetBuf.offset = 0;
                sendState.packetBuf.final = buffer;
            }
            SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }
    }
}