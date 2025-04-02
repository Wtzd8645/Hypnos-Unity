using System;
using System.Net.Sockets;

namespace Blanketmen.Hypnos.Network
{
    internal class MockSocket : SocketBase
    {
        public MockSocket(uint id, HandlerConfig handlerConfig) : base(id, handlerConfig) { }

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
            pendingResponses.Enqueue(responseProducer.Produce(request));
        }

        protected override void OnSendAsyncComplete(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}