using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Morpheus.Network
{
    internal delegate void ConnectionAoHandler(IConnection conn, SocketAsyncOperation operation, SocketError socketError);

    public partial class NetworkManager
    {
        public const int PacketLengthSize = sizeof(ushort);
        public const int SerialNumberSize = sizeof(byte);
        public const int MessageIdSize = sizeof(ushort);
        public static readonly Encoding StringEncoder = new UTF8Encoding(false, true);

        internal const int DefalutPort = 27015;
        internal const ushort DefalutMaxPacketSize = 1024; // byte
        internal const int DefalutSendTimeout = 4096; // ms
    }

    public enum TransportProtocol
    {
        LocalSimulation,
        TCP,
        UDP,
        RUDP,
        HTTP
    }

    public enum NetworkEvent
    {
        ConnectComplete,
        DisconnectComplete,
        ReceiveError,
        SendError
    }

    internal class NetworkEventComparer : IEqualityComparer<NetworkEvent>
    {
        public bool Equals(NetworkEvent x, NetworkEvent y)
        {
            return x == y;
        }

        public int GetHashCode(NetworkEvent obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class ConnectionEventArgs
    {
        public IConnection connection;
        public int version;
        public SocketAsyncOperation operation;
        public SocketError result; // Note: https://docs.microsoft.com/zh-tw/windows/win32/winsock/windows-sockets-error-codes-2
    }

    public class PacketBuffer
    {
        public int offset;
        public byte[] final;
        public byte[] compress;
        public byte[] encrypt;
    }

    internal class PacketReadState
    {
        public bool isWaitingPacketSize;
        public int waitingBytes;
        public int pendingBytes;
        public int processedBytes;
        public PacketBuffer packetBuf;
    }

    internal class PacketSendState
    {
        public bool isSending;
        public int pendingBytes;
        public int processedBytes;
        public PacketBuffer sendBuf;
        public PacketBuffer packetBuf;
    }
}