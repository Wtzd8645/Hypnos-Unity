using System;
using System.Net.Sockets;

namespace Morpheus.Network
{
    internal class TcpSocket : SocketBase
    {
        private static byte[] GetKeepAliveValue(int enable, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buf = new byte[12];
            BitConverter.GetBytes(enable).CopyTo(buf, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buf, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buf, 8);
            return buf;
        }

        private readonly SocketAsyncEventArgs connectEventArgs = new SocketAsyncEventArgs();

        public bool IsConnected => socket.Connected; // NOTE: Socket.Connected只會反應上一次的操作情況

        public TcpSocket(int id, SocketConnectionConfig conneConfig, SocketHandlerConfig handlerConfig) : base(id, conneConfig, handlerConfig)
        {
            CreateSocket();
            connectEventArgs.RemoteEndPoint = bindingEndPoint;
            connectEventArgs.Completed += OnConnectAsyncComplete;
        }

        private void CreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveValue(1, 8000, 2000), null);
            ++version;
        }

        public override void Dispose()
        {
            heartbeatTimer.Dispose();
            socket.Close();
        }

        public override void Reset()
        {
            DebugLogger.Log($"[TcpSocket] Reset. Id: {id}", (int)DebugLogChannel.Network);
            // NOTE: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.sockets.socket.close
            // socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            // heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public override void ConnectAsync()
        {
            try
            {
                DebugLogger.Log($"[TcpSocket] ConnectAsync. Id: {id}", (int)DebugLogChannel.Network);
                if (socket.Connected)
                {
                    onSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.IsConnected);
                    return;
                }

                if (!socket.ConnectAsync(connectEventArgs))
                {
                    OnConnectAsyncComplete(null, connectEventArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] ConnectAsync failed. Id: {id}, Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.SocketError);
            }
        }

        public override void DisconnectAsync()
        {
            try
            {
                DebugLogger.Log($"[TcpSocket] DisconnectAsync. Id: {id}", (int)DebugLogChannel.Network);
                if (!socket.Connected)
                {
                    onSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.NotConnected);
                    return;
                }

                socket.Shutdown(SocketShutdown.Both);
                if (!socket.DisconnectAsync(connectEventArgs))
                {
                    OnConnectAsyncComplete(null, connectEventArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] DisconnectAsync failed. Id: {id}, Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.SocketError);
            }
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            onSocketAoComplete(this, evtArgs.LastOperation, evtArgs.SocketError);
        }

        public override void ReceiveAsync()
        {
            DebugLogger.Log($"[TcpSocket] ReceiveAsync. Id: {id}", (int)DebugLogChannel.Network);
            ReceiveInternalAsync(socket, receiveEventArgs);
        }

        private void ReceiveInternalAsync(Socket connSocket, SocketAsyncEventArgs evtArgs)
        {
            try
            {
                if (!connSocket.ReceiveAsync(evtArgs))
                {
                    OnReceiveAsyncComplete(null, evtArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] ReceiveAsync failed. Id: {id}, Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.SocketError);
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

            DebugLogger.TraceLog($"[TcpSocket] Socket {id} received {evtArgs.BytesTransferred} bytes", (int)DebugLogChannel.Network);
            PacketReadState readState = evtArgs.UserToken as PacketReadState;
            readState.pendingBytes += evtArgs.BytesTransferred;
            while (readState.pendingBytes >= readState.waitingBytes)
            {
                if (readState.isWaitingPacketSize)
                {
                    readState.isWaitingPacketSize = false;
                    readState.waitingBytes = BitConverter.ToUInt16(evtArgs.Buffer, readState.processedBytes);
                    readState.pendingBytes -= NetworkManager.PacketLengthSize;
                    readState.processedBytes += NetworkManager.PacketLengthSize;
                    continue;
                }

                try
                {
                    readState.packetBuf.offset = readState.processedBytes;
                    onResponseComplete(this, responseProducer.Produce(readState.packetBuf));
                }
                catch (Exception e)
                {
                    DebugLogger.LogError($"[TcpSocket] Socket {id} create message failed. Exception: {e.Message}");
                    onSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
                }
                finally
                {
                    readState.pendingBytes -= readState.waitingBytes;
                    readState.processedBytes += readState.waitingBytes;
                    readState.isWaitingPacketSize = true;
                    readState.waitingBytes = NetworkManager.PacketLengthSize;
                }
            }

            if (readState.pendingBytes > 0)
            {
                Buffer.BlockCopy(evtArgs.Buffer, readState.processedBytes, evtArgs.Buffer, 0, readState.pendingBytes);
            }

            readState.processedBytes = 0;
            evtArgs.SetBuffer(readState.pendingBytes, receiveBufferSize - readState.pendingBytes);
            ReceiveInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }

        public override void SendAsync(IRequest request)
        {
            if (!IsConnected) // NOTE: Sending in a disconnected state will cause an exception to the subsequent connection sending.
            {
                return;
            }

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
                    DebugLogger.TraceLog($"[TcpSocket] Socket {id} produce {producedBytes} bytes.", (int)DebugLogChannel.Network);
                    return;
                }
            }

            sendState.isSending = true;
            sendState.pendingBytes = request.Pack(sendState.packetBuf);
            sendState.processedBytes = 0;
            if (sendState.pendingBytes > maxPacketSize)
            {
                onSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                return;
            }

            DebugLogger.TraceLog($"[TcpSocket] Socket {id} send {sendState.pendingBytes} bytes.", (int)DebugLogChannel.Network);
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
                if (!connSocket.SendAsync(evtArgs))
                {
                    OnSendAsyncComplete(null, evtArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] SendAsync failed. Id: {id}, Exception: {e.Message}");
                onSocketAoComplete(this, SocketAsyncOperation.Send, evtArgs.SocketError);
            }
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            DebugLogger.TraceLog($"[TcpSocket] Socket {id} sent {evtArgs.BytesTransferred} bytes.", (int)DebugLogChannel.Network);
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

            lock (evtArgs)
            {
                if (sendState.packetBuf.offset == 0)
                {
                    sendState.isSending = false;
                    return;
                }

                DebugLogger.TraceLog($"[TcpSocket] Socket {id} send {sendState.packetBuf.offset} produced bytes.", (int)DebugLogChannel.Network);
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