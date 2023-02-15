using System;
using System.Net.Sockets;

namespace Hypnos.Network
{
    internal class UdpSocket : SocketBase
    {
        public UdpSocket(int id, TransportConfig transportConfig, HandlerConfig handlerConfig) : base(id, transportConfig, handlerConfig)
        {
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
        }

        public override void Dispose()
        {
            heartbeatTimer.Dispose();
            socket.Close();
        }

        public override void Reset()
        {
            Kernel.Log($"[UdpSocket] Reset. Id: {id}", (int)LogChannel.Network);
            socket.Close(); // NOTE: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.sockets.socket.close
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            // heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ++version;
        }

        protected override void CreateReceiveEventArgs()
        {
            base.CreateReceiveEventArgs();
            receiveEventArgs.RemoteEndPoint = bindingEndPoint;
        }

        protected override void CreateSendEventArgs()
        {
            base.CreateSendEventArgs();
            sendEventArgs.RemoteEndPoint = bindingEndPoint;
        }

        public override void ConnectAsync()
        {
            onConnectionAoComplete(this, SocketAsyncOperation.Connect, SocketError.Success);
        }

        public override void DisconnectAsync()
        {
            onConnectionAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.Success);
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs evtArgs) { }

        public override void ReceiveAsync()
        {
            Kernel.Log($"[UdpSocket] ReceiveAsync. Id: {id}", (int)LogChannel.Network);
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
                Kernel.LogError($"[UdpSocket] ReceiveAsync failed. Id: {id}, Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Receive, evtArgs.SocketError);
            }
        }

        protected override void OnReceiveAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            if (evtArgs.SocketError != SocketError.Success) // Abnormal shutdown.
            {
                onConnectionAoComplete(this, SocketAsyncOperation.Receive, evtArgs.SocketError);
                return;
            }

            if (evtArgs.BytesTransferred == 0) // Normal shutdown.
            {
                onConnectionAoComplete(this, SocketAsyncOperation.Receive, SocketError.Disconnecting);
                return;
            }

            Kernel.TraceLog($"[UdpSocket] Socket {id} received {evtArgs.BytesTransferred} bytes", (int)LogChannel.Network);
            try
            {
                PacketReadState readState = evtArgs.UserToken as PacketReadState;
                readState.packetBuf.offset = NetworkManager.PacketLengthSize;
                pendingResponses.Enqueue(responseProducer.Produce(readState.packetBuf));
            }
            catch (Exception e)
            {
                Kernel.LogError($"[UdpSocket] Socket {id} create message failed. Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
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
                    int pendingBytes = sendState.packetBuf.offset + packetBytes;
                    if (packetBytes > maxPacketSize || pendingBytes > sendState.packetBuf.final.Length)
                    {
                        onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                        return;
                    }

                    sendState.packetBuf.offset = pendingBytes;
                    Kernel.TraceLog($"[UdpSocket] Socket {id} produce {pendingBytes} bytes.", (int)LogChannel.Network);
                    return;
                }
            }

            sendState.isSending = true;
            sendState.pendingBytes = request.Pack(sendState.sendBuf);
            sendState.processedBytes = 0;
            if (sendState.pendingBytes > maxPacketSize)
            {
                onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                return;
            }

            Kernel.TraceLog($"[UdpSocket] Socket {id} send {sendState.pendingBytes} bytes.", (int)LogChannel.Network);
            sendEventArgs.SetBuffer(0, sendState.pendingBytes);
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
                Kernel.LogError($"[UdpSocket] SendAsync failed. Id: {id}, Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.SocketError);
            }
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            Kernel.TraceLog($"[UdpSocket] Socket {id} sent {evtArgs.BytesTransferred} bytes.", (int)LogChannel.Network);
            PacketSendState sendState = evtArgs.UserToken as PacketSendState;
            if (evtArgs.SocketError != SocketError.Success)
            {
                onConnectionAoComplete(this, SocketAsyncOperation.Send, evtArgs.SocketError);
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

                Kernel.TraceLog($"[UdpSocket] Socket {id} send {sendState.packetBuf.offset} produced bytes.", (int)LogChannel.Network);
                evtArgs.SetBuffer(sendState.packetBuf.final, 0, sendState.packetBuf.offset);
                sendState.pendingBytes = sendState.packetBuf.offset;
                sendState.processedBytes = 0;

                byte[] sendBuf = sendState.sendBuf.final;
                sendState.sendBuf.final = sendState.packetBuf.final;
                sendState.packetBuf.offset = 0;
                sendState.packetBuf.final = sendBuf;
            }
            SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }
    }
}