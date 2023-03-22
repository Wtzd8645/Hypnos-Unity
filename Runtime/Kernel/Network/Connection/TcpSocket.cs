using System;
using System.Net.Sockets;

namespace Blanketmen.Hypnos
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

        public TcpSocket(int id, TransportConfig transportConfig, HandlerConfig handlerConfig) : base(id, transportConfig, handlerConfig)
        {
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            connectEventArgs.RemoteEndPoint = bindingEndPoint;
            connectEventArgs.Completed += OnConnectAsyncComplete;
        }

        public override void Dispose()
        {
            heartbeatTimer.Dispose();
            socket.Close();
        }

        public override void Reset()
        {
            Kernel.Log($"[TcpSocket] Reset. Id: {id}", (int)LogChannel.Network);
            socket.Close(); // NOTE: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.sockets.socket.close
            CreateSocket();
            CreateReceiveEventArgs();
            CreateSendEventArgs();
            // heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };
            // socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveValue(1, 8000, 2000), null);
            ++version;
        }

        public override void ConnectAsync()
        {
            try
            {
                Kernel.Log($"[TcpSocket] ConnectAsync. Id: {id}", (int)LogChannel.Network);
                if (socket.Connected)
                {
                    onConnectionAoComplete(this, SocketAsyncOperation.Connect, SocketError.IsConnected);
                    return;
                }

                if (!socket.ConnectAsync(connectEventArgs))
                {
                    OnConnectAsyncComplete(null, connectEventArgs);
                }
            }
            catch (Exception e)
            {
                Kernel.LogError($"[TcpSocket] ConnectAsync failed. Id: {id}, Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Connect, SocketError.SocketError);
            }
        }

        public override void DisconnectAsync()
        {
            try
            {
                Kernel.Log($"[TcpSocket] DisconnectAsync. Id: {id}", (int)LogChannel.Network);
                if (!socket.Connected)
                {
                    onConnectionAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.NotConnected);
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
                Kernel.LogError($"[TcpSocket] DisconnectAsync failed. Id: {id}, Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.SocketError);
            }
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            onConnectionAoComplete(this, evtArgs.LastOperation, evtArgs.SocketError);
        }

        public override void ReceiveAsync()
        {
            Kernel.Log($"[TcpSocket] ReceiveAsync. Id: {id}", (int)LogChannel.Network);
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
                Kernel.LogError($"[TcpSocket] ReceiveAsync failed. Id: {id}, Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Receive, SocketError.SocketError);
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

            Kernel.TraceLog($"[TcpSocket] Socket {id} received {evtArgs.BytesTransferred} bytes", (int)LogChannel.Network);
            PacketReadState readState = evtArgs.UserToken as PacketReadState;
            readState.pendingBytes += evtArgs.BytesTransferred;
            while (readState.pendingBytes >= readState.waitingBytes)
            {
                if (readState.isWaitingPacketSize)
                {
                    readState.isWaitingPacketSize = false;
                    readState.waitingBytes = BitConverter.ToInt16(evtArgs.Buffer, readState.processedBytes);
                    readState.pendingBytes -= NetworkManager.PacketLengthSize;
                    readState.processedBytes += NetworkManager.PacketLengthSize;
                    continue;
                }

                try
                {
                    readState.packetBuf.offset = readState.processedBytes;
                    pendingResponses.Enqueue(responseProducer.Produce(readState.packetBuf));
                }
                catch (Exception e)
                {
                    Kernel.LogError($"[TcpSocket] Socket {id} create message failed. Exception: {e.Message}");
                    onConnectionAoComplete(this, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
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
                    int pendingBytes = sendState.packetBuf.offset + packetBytes;
                    if (packetBytes > maxPacketSize || pendingBytes > sendState.packetBuf.final.Length)
                    {
                        onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
                        return;
                    }

                    sendState.packetBuf.offset = pendingBytes;
                    Kernel.TraceLog($"[TcpSocket] Socket {id} produce {pendingBytes} bytes.", (int)LogChannel.Network);
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

            Kernel.TraceLog($"[TcpSocket] Socket {id} send {sendState.pendingBytes} bytes.", (int)LogChannel.Network);
            sendEventArgs.SetBuffer(0, sendState.pendingBytes);
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
                Kernel.LogError($"[TcpSocket] SendAsync failed. Id: {id}, Exception: {e.Message}");
                onConnectionAoComplete(this, SocketAsyncOperation.Send, evtArgs.SocketError);
            }
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs evtArgs)
        {
            Kernel.TraceLog($"[TcpSocket] Socket {id} sent {evtArgs.BytesTransferred} bytes.", (int)LogChannel.Network);
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

            lock (evtArgs)
            {
                if (sendState.packetBuf.offset == 0)
                {
                    sendState.isSending = false;
                    return;
                }

                Kernel.TraceLog($"[TcpSocket] Socket {id} send {sendState.packetBuf.offset} produced bytes.", (int)LogChannel.Network);
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