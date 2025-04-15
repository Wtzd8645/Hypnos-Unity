using System.Collections.Generic;
using System.Net.Sockets;

namespace Blanketmen.Hypnos.Network
{
    internal class RecvContext
    {
        public int receivedBytes;
        public int processedBytes;
        public uint packetBytes;
        public uint waitingBytes;
    }

    internal class SendContext
    {
        public int pendingBytes;
        public int processedBytes;
        public Queue<byte[]> pendingPackets = new Queue<byte[]>(8);
    }

    public class Connection
    {
        public Socket sock = null;
        public uint version = 0;
        internal RecvContext recvCtx = new RecvContext();
        internal SendContext sendCtx = new SendContext();
    }

    public struct ConnectionHandle
    {
        public Connection conn;
        public uint version;
    }

    internal class ConnectionContext
    {
        public Connection conn = new Connection();
        public SocketAsyncEventArgs recvEvtArgs = new SocketAsyncEventArgs();
        public SocketAsyncEventArgs sendEvtArgs = new SocketAsyncEventArgs();
    }
}