using System;
using System.Net.Sockets;

namespace Blanketmen.Hypnos.Network
{
    internal class UdpSocket : ClientSocketBase
    {
        public UdpSocket(SocketConfig cfg) : base(cfg)
        {
            CreateSocket();
            //CreateReceiveEventArgs();
            //CreateSendEventArgs();
        }

        public void Dispose()
        {
            //heartbeatTimer.Dispose();
            socket.Close();
        }

        public void Reset()
        {
            Logging.Info($"Reset. Id: {id}", (int)LogChannel.Network);
            socket.Close(); // NOTE: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.sockets.socket.close
            CreateSocket();
            //CreateReceiveEventArgs();
            //CreateSendEventArgs();
            //heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ++version;
        }

        //protected override void CreateReceiveEventArgs()
        //{
        //    base.CreateReceiveEventArgs();
        //    recvEventArgs.RemoteEndPoint = bindingEndPoint;
        //}

        //protected override void CreateSendEventArgs()
        //{
        //    base.CreateSendEventArgs();
        //    sendEventArgs.RemoteEndPoint = bindingEndPoint;
        //}

        public override void Start()
        {
            //onSocketEvent(id, SocketAsyncOperation.Connect, SocketError.Success);
        }

        public override void Stop()
        {
            //onSocketEvent(id, SocketAsyncOperation.Disconnect, SocketError.Success);
        }

        public override void Process(IOEventArgs args)
        {
            //switch (args.op)
            //{
            //    case SocketOperation.Receive:
            //    {
            //        OnReceiveComplete(null);
            //        break;
            //    }
            //    case SocketOperation.Send:
            //    {
            //        OnSendComplete(null);
            //        break;
            //    }
            //    default:
            //    {
            //        Logging.Error($"Invalid socket operation. Id: {id}, Operation: {args.op}", nameof(UdpSocket));
            //        break;
            //    }
            //}
        }

        public void ReceiveInternalAsync()
        {
            Logging.Info($"ReceiveAsync. Id: {id}", (int)LogChannel.Network);
            //recvEventArgs.SetBuffer(0, NetworkDefs.MAX_BUFFER_SIZE);
            //ReceiveInternalAsync(socket, recvEventArgs);
        }

        private void ReceiveInternalAsync(Socket connSocket, SocketAsyncEventArgs args)
        {
            try
            {
                if (!connSocket.ReceiveFromAsync(args))
                {
                    OnIOComplete(null, args);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"ReceiveAsync failed. Id: {id}, Exception: {e.Message}", nameof(UdpSocket));
                //onSocketEvent(id, SocketAsyncOperation.Receive, args.SocketError);
            }
        }

        protected void OnReceiveComplete(SocketAsyncEventArgs evtArgs)
        {
            if (evtArgs.SocketError != SocketError.Success) // Abnormal shutdown.
            {
                //onSocketEvent(id, SocketAsyncOperation.Receive, evtArgs.SocketError);
                return;
            }

            if (evtArgs.BytesTransferred == 0) // Normal shutdown.
            {
                //onSocketEvent(id, SocketAsyncOperation.Receive, SocketError.Disconnecting);
                return;
            }

            Logging.Trace($"[UdpSocket] Socket {id} received {evtArgs.BytesTransferred} bytes", (int)LogChannel.Network);
            try
            {
                RecvContext readState = evtArgs.UserToken as RecvContext;
                //readState.packetBuf.offset = NetworkDefs.PACKET_SIZE_LENGTH;
                //pendingResponses.Enqueue(responsePool.Produce(readState.packetBuf));
            }
            catch (Exception e)
            {
                Logging.Error($"Socket {id} create message failed. Exception: {e.Message}", nameof(UdpSocket));
                //onSocketEvent(id, SocketAsyncOperation.Receive, SocketError.TypeNotFound);
            }

            evtArgs.SetBuffer(0, NetworkDefs.MAX_BUFFER_SIZE);
            ReceiveInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }

        public override void Send(IRequest request)
        {
            //SendContext sendState = sendEventArgs.UserToken as SendContext;
            //lock (sendEventArgs)
            //{
            //    if (sendState.isSending)
            //    {
            //        int packetBytes = request.Pack(sendState.packetBuf);
            //        int pendingBytes = sendState.packetBuf.offset + packetBytes;
            //        if (packetBytes > NetworkDefs.MAX_PACKET_SIZE || pendingBytes > sendState.packetBuf.final.Length)
            //        {
            //            onSocketAoComplete(id, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
            //            return;
            //        }

            //        sendState.packetBuf.offset = pendingBytes;
            //        Logging.Trace($"[UdpSocket] Socket {id} produce {pendingBytes} bytes.", (int)LogChannel.Network);
            //        return;
            //    }
            //}

            //sendState.isSending = true;
            //sendState.pendingBytes = request.Pack(sendState.sendBuf);
            //sendState.processedBytes = 0;
            //if (sendState.pendingBytes > NetworkDefs.MAX_PACKET_SIZE)
            //{
            //    onSocketAoComplete(id, SocketAsyncOperation.Send, SocketError.NoBufferSpaceAvailable);
            //    return;
            //}

            //Logging.Trace($"[UdpSocket] Socket {id} send {sendState.pendingBytes} bytes.", (int)LogChannel.Network);
            //sendEventArgs.SetBuffer(0, sendState.pendingBytes);
            //SendInternalAsync(socket, sendEventArgs);
        }

        private void SendInternalAsync(Socket connSocket, SocketAsyncEventArgs args)
        {
            try
            {
                if (!connSocket.SendToAsync(args))
                {
                    OnIOComplete(null, args);
                }
            }
            catch (Exception e)
            {
                Logging.Error($"SendAsync failed. Id: {id}, Exception: {e.Message}", nameof(UdpSocket));
                //onSocketEvent(id, SocketAsyncOperation.Send, SocketError.SocketError);
            }
        }

        protected void OnSendComplete(SocketAsyncEventArgs evtArgs)
        {
            //Logging.Trace($"[UdpSocket] Socket {id} sent {evtArgs.BytesTransferred} bytes.", (int)LogChannel.Network);
            //SendContext sendState = evtArgs.UserToken as SendContext;
            //if (evtArgs.SocketError != SocketError.Success)
            //{
            //    onSocketAoComplete(id, SocketAsyncOperation.Send, evtArgs.SocketError);
            //    return;
            //}

            //sendState.pendingBytes -= evtArgs.BytesTransferred;
            //if (sendState.pendingBytes > 0)
            //{
            //    sendState.processedBytes += evtArgs.BytesTransferred;
            //    evtArgs.SetBuffer(sendState.processedBytes, sendState.pendingBytes);
            //    SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
            //    return;
            //}

            //lock (sendEventArgs)
            //{
            //    if (sendState.packetBuf.offset == 0)
            //    {
            //        sendState.isSending = false;
            //        return;
            //    }

            //    Logging.Trace($"[UdpSocket] Socket {id} send {sendState.packetBuf.offset} produced bytes.", (int)LogChannel.Network);
            //    evtArgs.SetBuffer(sendState.packetBuf.final, 0, sendState.packetBuf.offset);
            //    sendState.pendingBytes = sendState.packetBuf.offset;
            //    sendState.processedBytes = 0;

            //    byte[] sendBuf = sendState.sendBuf.final;
            //    sendState.sendBuf.final = sendState.packetBuf.final;
            //    sendState.packetBuf.offset = 0;
            //    sendState.packetBuf.final = sendBuf;
            //}
            //SendInternalAsync(evtArgs.ConnectSocket, evtArgs);
        }
    }
}