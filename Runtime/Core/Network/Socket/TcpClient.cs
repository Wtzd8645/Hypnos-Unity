using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;

namespace Blanketmen.Hypnos.Network
{
    internal class TcpClient : ClientSocketBase
    {
        private static byte[] GetKeepAliveValue(int enable, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buf = new byte[12];
            BitConverter.GetBytes(enable).CopyTo(buf, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buf, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buf, 8);
            return buf;
        }

        private readonly IOEventArgs pollArgs = new IOEventArgs();
        private readonly SocketAsyncEventArgs connArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        private readonly RecvContext recvCtx = new RecvContext();
        private readonly SendContext sendCtx = new SendContext();

        public bool IsConnected => socket != null && socket.Connected; // NOTE: Socket.Connected only reflects last operation status.

        public TcpClient(SocketConfig cfg) : base(cfg)
        {
            pollArgs.sockHandle = new SocketHandle
            {
                sock = this,
                version = version
            };
            pollArgs.op = IOEventArgs.Operation.None;

            connArgs.RemoteEndPoint = endPoint;
            connArgs.UserToken = new IOEventArgs
            {
                sockHandle = new SocketHandle
                {
                    sock = this,
                    version = version
                },
                op = IOEventArgs.Operation.Connect
            };
            connArgs.Completed += OnIOComplete;

            recvArgs.UserToken = new IOEventArgs
            {
                sockHandle = new SocketHandle
                {
                    sock = this,
                    version = version
                },
                op = IOEventArgs.Operation.Receive
            };
            recvArgs.SetBuffer(new byte[NetworkDefs.MAX_BUFFER_SIZE], 0, NetworkDefs.MAX_BUFFER_SIZE);
            recvArgs.Completed += OnIOComplete;

            sendArgs.UserToken = new IOEventArgs
            {
                sockHandle = new SocketHandle
                {
                    sock = this,
                    version = version
                },
                op = IOEventArgs.Operation.Send
            };
            sendArgs.Completed += OnIOComplete;
        }

        public override void Start()
        {
            try
            {
                Logging.Info($"Start client socket. Id: {id}", (int)LogChannel.Network);
                if (socket != null)
                {
                    OnSocketEvent(ClientEvent.Type.Connect, SocketError.IsConnected);
                    return;
                }

                if (Socket.OSSupportsIPv6)
                {
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
                    {
                        DualMode = true, // Enable dual mode for IPv4 and IPv6.
                        NoDelay = true
                    };
                }
                else
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        NoDelay = true
                    };
                }
                // socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveValue(1, 8000, 2000), null);

                if (!socket.ConnectAsync(connArgs))
                {
                    OnIOComplete(null, connArgs);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Start client socket failed. Id: {id}, Exception: {e.Message}", nameof(TcpClient));
                OnSocketEvent(ClientEvent.Type.Connect, SocketError.SocketError);
            }
        }

        public override void Stop()
        {
            try
            {
                Logging.Info($"Stop client socket. Id: {id}", (int)LogChannel.Network);
                if (socket == null)
                {
                    OnSocketEvent(ClientEvent.Type.Disconnect, SocketError.NotConnected);
                    return;
                }

                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Logging.Error($"Stop client socket. Id: {id}, Exception: {e.Message}", nameof(TcpClient));
                OnSocketEvent(ClientEvent.Type.Disconnect, SocketError.SocketError);
            }
        }

        public override void Send(IRequest req)
        {
            if (!IsConnected)
            {
                return;
            }

            byte[] buf = ArrayPool<byte>.Shared.Rent(NetworkDefs.MAX_BUFFER_SIZE);
            RequestEventArgs args = new RequestEventArgs
            {
                buffer = buf,
                length = req.Pack(buf),
            };

            if (args.length > NetworkDefs.MAX_PACKET_SIZE)
            {
                Logging.Error($"Send message size too large. Id: {req.Id}, Size: {args.length}", nameof(TcpClient));
                return;
            }

            requestArgs.Enqueue(args);
            ioContext.events.Enqueue(pollArgs);
            ioContext.signal.Set();
        }

        public override void Process(IOEventArgs args)
        {
            switch (args.op)
            {
                case IOEventArgs.Operation.None:
                {
                    ProcessRequests();
                    break;
                }
                case IOEventArgs.Operation.Connect:
                {
                    OnConnectComplete(connArgs);
                    break;
                }
                case IOEventArgs.Operation.Receive:
                {
                    OnReceiveComplete(recvArgs);
                    break;
                }
                case IOEventArgs.Operation.Send:
                {
                    OnSendComplete(sendArgs);
                    break;
                }
            }
        }

        private void OnConnectComplete(SocketAsyncEventArgs args)
        {
            Logging.Info($"Client socket connect complete. Id: {id}, Error: {args.SocketError}", (int)LogChannel.Network);
            if (args.SocketError == SocketError.Success)
            {
                ReceiveInternal(args.ConnectSocket, recvArgs);
            }

            OnSocketEvent(ClientEvent.Type.Connect, args.SocketError);
        }

        private void ProcessRequests()
        {
            if (sendCtx.pendingBytes == 0)
            {
                RequestEventArgs args;
                while (!requestArgs.TryDequeue(out args))
                {
                    Thread.SpinWait(4);
                }

                sendCtx.pendingBytes = args.length;
                sendArgs.SetBuffer(args.buffer, 0, args.length);
                SendInternal(socket, sendArgs);
            }
        }

        private void ReceiveInternal(Socket sock, SocketAsyncEventArgs args)
        {
            try
            {
                if (!sock.ReceiveAsync(args))
                {
                    OnReceiveComplete(args);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Receive failed. Id: {id}, Exception: {e.Message}", nameof(TcpClient));
                CloseInternal(SocketAsyncOperation.Receive, args.SocketError);
            }
        }

        private void OnReceiveComplete(SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success) // Abnormal shutdown.
            {
                CloseInternal(SocketAsyncOperation.Receive, args.SocketError);
                return;
            }

            if (args.BytesTransferred == 0) // Normal shutdown.
            {
                CloseInternal(SocketAsyncOperation.Receive, SocketError.Disconnecting);
                return;
            }

            Logging.Trace($"[TcpSocket] Client socket receive complete. Id: {id}, Bytes: {args.BytesTransferred}", (int)LogChannel.Network);
            recvCtx.receivedBytes += args.BytesTransferred;
            while (recvCtx.receivedBytes >= recvCtx.waitingBytes)
            {
                if (recvCtx.packetBytes == 0)
                {
                    recvCtx.packetBytes = BitConverter.ToUInt16(args.Buffer, recvCtx.processedBytes);
                    if (recvCtx.packetBytes > NetworkDefs.MAX_PACKET_SIZE)
                    {
                        Logging.Error($"Received packet size too large. Id: {id}, Size: {recvCtx.packetBytes}", nameof(TcpClient));
                        CloseInternal(SocketAsyncOperation.Receive, SocketError.MessageSize);
                        return;
                    }

                    recvCtx.waitingBytes = recvCtx.packetBytes;
                    recvCtx.receivedBytes -= NetworkDefs.PACKET_SIZE_LENGTH;
                    recvCtx.processedBytes += NetworkDefs.PACKET_SIZE_LENGTH;
                    continue;
                }

                try
                {
                    IResponse resp = responseAlloctor.Acquire(new Span<byte>(args.Buffer, recvCtx.processedBytes, (int)recvCtx.packetBytes));
                    responses.Enqueue(resp);
                    recvCtx.receivedBytes -= (int)recvCtx.packetBytes;
                    recvCtx.processedBytes += (int)recvCtx.packetBytes;
                    recvCtx.packetBytes = 0;
                    recvCtx.waitingBytes = NetworkDefs.PACKET_SIZE_LENGTH;
                }
                catch (Exception e)
                {
                    Logging.Error($"Failed to process packet. Id: {id}, Exception: {e.Message}", nameof(TcpClient));
                    CloseInternal(SocketAsyncOperation.Receive, SocketError.TypeNotFound);
                    return;
                }
            }

            if (recvCtx.receivedBytes > 0 && recvCtx.processedBytes > 0)
            {
                Buffer.BlockCopy(args.Buffer, recvCtx.processedBytes, args.Buffer, 0, recvCtx.receivedBytes);
            }

            recvCtx.processedBytes = 0;
            args.SetBuffer(recvCtx.receivedBytes, NetworkDefs.MAX_BUFFER_SIZE - recvCtx.receivedBytes);
            ReceiveInternal(args.ConnectSocket, args);
        }

        private void SendInternal(Socket sock, SocketAsyncEventArgs args)
        {
            try
            {
                if (!sock.SendAsync(args))
                {
                    OnSendComplete(args);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Send failed. Id: {id}, Exception: {e.Message}", nameof(TcpClient));
                CloseInternal(SocketAsyncOperation.Send, args.SocketError);
            }
        }

        private void OnSendComplete(SocketAsyncEventArgs args)
        {
            Logging.Trace($"[TcpSocket] Client socket send complete. Id: {id}, Bytes: {args.BytesTransferred}", (int)LogChannel.Network);
            if (args.SocketError != SocketError.Success)
            {
                CloseInternal(SocketAsyncOperation.Send, args.SocketError);
                return;
            }

            sendCtx.pendingBytes -= args.BytesTransferred;
            if (sendCtx.pendingBytes > 0)
            {
                sendCtx.processedBytes += args.BytesTransferred;
                args.SetBuffer(sendCtx.processedBytes, sendCtx.pendingBytes);
                SendInternal(args.ConnectSocket, args);
                return;
            }

            ArrayPool<byte>.Shared.Return(args.Buffer);
            if (requestArgs.IsEmpty)
            {
                return;
            }

            RequestEventArgs reqArgs;
            while (!requestArgs.TryDequeue(out reqArgs))
            {
                Thread.SpinWait(4);
            }

            sendCtx.pendingBytes = reqArgs.length;
            sendArgs.SetBuffer(reqArgs.buffer, 0, reqArgs.length);
            SendInternal(args.ConnectSocket, args);
        }
    }
}