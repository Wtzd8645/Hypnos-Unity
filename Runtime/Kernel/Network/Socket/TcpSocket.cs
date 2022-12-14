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

        public bool IsConnected => Socket.Connected; // NOTE: Socket.Connected只會反應上一次的操作情況

        public TcpSocket(int id, SocketConnectionConfig conneConfig, SocketHandlerConfig handlerConfig) : base(id, conneConfig, handlerConfig)
        {
            CreateSocket();
            connectEventArgs.RemoteEndPoint = BindingEndPoint;
            connectEventArgs.Completed += OnConnectAsyncComplete;
        }

        private void CreateSocket()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.NoDelay = true;
            Socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveValue(1, 8000, 2000), null);
            ++Version;
        }

        public override void Dispose()
        {
            HeartbeatTimer.Dispose();
            Socket.Close();
        }

        public override void Reset()
        {
            DebugLogger.Log($"[TcpSocket] Reset. Id: {Id}", (int)DebugLogChannel.Network);
            // NOTE: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.sockets.socket.close
            // socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            // heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public override void ConnectAsync()
        {
            try
            {
                DebugLogger.Log($"[TcpSocket] ConnectAsync. Id: {Id}", (int)DebugLogChannel.Network);
                if (Socket.Connected)
                {
                    OnSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.IsConnected);
                    return;
                }

                if (!Socket.ConnectAsync(connectEventArgs))
                {
                    OnConnectAsyncComplete(null, connectEventArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] ConnectAsync failed. Id: {Id}, Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.SocketError);
            }
        }

        public override void DisconnectAsync()
        {
            try
            {
                DebugLogger.Log($"[TcpSocket] DisconnectAsync. Id: {Id}", (int)DebugLogChannel.Network);
                if (!Socket.Connected)
                {
                    OnSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.NotConnected);
                    return;
                }

                Socket.Shutdown(SocketShutdown.Both);
                if (!Socket.DisconnectAsync(connectEventArgs))
                {
                    OnConnectAsyncComplete(null, connectEventArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] DisconnectAsync failed. Id: {Id}, Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.SocketError);
            }
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            OnSocketAoComplete(this, evtArgs.LastOperation, evtArgs.SocketError);
        }

        public override void ReceiveAsync()
        {
            DebugLogger.Log($"[TcpSocket] ReceiveAsync. Id: {Id}", (int)DebugLogChannel.Network);
            ReceiveInternalAsync(Socket, ReceiveEventArgs);
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
                DebugLogger.LogError($"[TcpSocket] ReceiveAsync failed. Id: {Id}, Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.SocketError);
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

            DebugLogger.TraceLog($"[TcpSocket] Socket {Id} received {evtArgs.BytesTransferred} bytes", (int)DebugLogChannel.Network);
            PacketReadState readState = evtArgs.UserToken as PacketReadState;
            readState.PendingBytes += evtArgs.BytesTransferred;
            while (readState.PendingBytes >= readState.WaitingBytes)
            {
                if (readState.IsWaitingPacketSize)
                {
                    readState.IsWaitingPacketSize = false;
                    readState.WaitingBytes = BitConverter.ToUInt16(evtArgs.Buffer, readState.ProcessedBytes);
                    readState.PendingBytes -= NetworkManager.PacketLengthSize;
                    readState.ProcessedBytes += NetworkManager.PacketLengthSize;
                    continue;
                }

                try
                {
                    readState.PacketBuf.Offset = readState.ProcessedBytes;
                    OnResponseComplete(this, ResponseProducer.Produce(readState.PacketBuf));
                }
                catch (Exception e)
                {
                    DebugLogger.LogError($"[TcpSocket] Socket {Id} create message failed. Exception: {e.Message}");
                    OnSocketAoComplete(this, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
                }
                finally
                {
                    readState.PendingBytes -= readState.WaitingBytes;
                    readState.ProcessedBytes += readState.WaitingBytes;
                    readState.IsWaitingPacketSize = true;
                    readState.WaitingBytes = NetworkManager.PacketLengthSize;
                }
            }

            if (readState.PendingBytes > 0)
            {
                Buffer.BlockCopy(evtArgs.Buffer, readState.ProcessedBytes, evtArgs.Buffer, 0, readState.PendingBytes);
            }

            readState.ProcessedBytes = 0;
            evtArgs.SetBuffer(readState.PendingBytes, ReceiveBufferSize - readState.PendingBytes);
            ReceiveInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }

        public override void SendAsync(IRequest request)
        {
            if (!IsConnected) // NOTE: Sending in a disconnected state will cause an exception to the subsequent connection sending.
            {
                return;
            }

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
                    DebugLogger.TraceLog($"[TcpSocket] Socket {Id} produce {producedBytes} bytes.", (int)DebugLogChannel.Network);
                    return;
                }
            }

            sendState.IsSending = true;
            sendState.PendingBytes = request.Pack(sendState.PacketBuf);
            sendState.ProcessedBytes = 0;
            if (sendState.PendingBytes > MaxPacketSize)
            {
                OnSocketAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                return;
            }

            DebugLogger.TraceLog($"[TcpSocket] Socket {Id} send {sendState.PendingBytes} bytes.", (int)DebugLogChannel.Network);
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
                if (!connSocket.SendAsync(evtArgs))
                {
                    OnSendAsyncComplete(null, evtArgs);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[TcpSocket] SendAsync failed. Id: {Id}, Exception: {e.Message}");
                OnSocketAoComplete(this, SocketAsyncOperation.Send, evtArgs.SocketError);
            }
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            DebugLogger.TraceLog($"[TcpSocket] Socket {Id} sent {evtArgs.BytesTransferred} bytes.", (int)DebugLogChannel.Network);
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

            lock (evtArgs)
            {
                if (sendState.PacketBuf.Offset == 0)
                {
                    sendState.IsSending = false;
                    return;
                }

                DebugLogger.TraceLog($"[TcpSocket] Socket {Id} send {sendState.PacketBuf.Offset} produced bytes.", (int)DebugLogChannel.Network);
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