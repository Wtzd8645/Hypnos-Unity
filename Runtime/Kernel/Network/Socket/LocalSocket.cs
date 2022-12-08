using System;
using System.Net.Sockets;

namespace Morpheus.Network
{
    internal class LocalSocket : SocketBase
    {
        public LocalSocket(int id, SocketHandlerConfig handlerConfig) : base(id, handlerConfig) { }

        public override void Dispose() { }

        public override void Reset() { }

        public override void ConnectAsync()
        {
            onSocketAoComplete(this, SocketAsyncOperation.Connect, SocketError.Success);
        }

        public override void DisconnectAsync()
        {
            onSocketAoComplete(this, SocketAsyncOperation.Disconnect, SocketError.Success);
        }

        protected override void OnConnectAsyncComplete(object sender, SocketAsyncEventArgs e)
        {
            onSocketAoComplete(this, e.LastOperation, SocketError.Success);
        }

        public override void ReceiveAsync() { }

        protected override void OnReceiveAsyncComplete(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void SendAsync(IRequest request)
        {
            onResponseComplete(this, responseProducer.Produce(request));
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}