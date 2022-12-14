using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Morpheus.Network
{
    internal delegate void SocketResponseHandler(ISocket socket, IResponse response);
    internal delegate void SocketAoHandler(SocketBase socket, SocketAsyncOperation operation, SocketError socketError);

    public partial class NetworkManager
    {
        public const int PacketLengthSize = sizeof(ushort);
        public const int SerialNumberSize = sizeof(byte);
        public const int MessageIdSize = sizeof(ushort);
        public static readonly Encoding StringEncoder = new UTF8Encoding(false, true);

        internal const int DefalutPort = 27015;
        internal const ushort DefalutMaxBufferSize = 1024; // byte
        internal const int DefalutSendTimeout = 4096; // ms
    }

    public enum TransportProtocol
    {
        LocalSimulation,
        Tcp,
        Udp,
        Rudp,
        Http
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

    internal class SocketEventArgs
    {
        public SocketBase Socket;
        public int SocketVersion;
        public SocketAsyncOperation Operation;
        public SocketError Result; // Note: https://docs.microsoft.com/zh-tw/windows/win32/winsock/windows-sockets-error-codes-2
    }

    public class PacketBuffer
    {
        public int Offset;
        public byte[] Final;
        public byte[] Compress;
        public byte[] Encrypt;
    }

    internal class PacketReadState
    {
        public bool IsWaitingPacketSize;
        public int WaitingBytes;
        public int PendingBytes;
        public int ProcessedBytes;
        public PacketBuffer PacketBuf;
    }

    internal class PacketSendState
    {
        public bool IsSending;
        public int PendingBytes;
        public int ProcessedBytes;
        public byte[] SendBuf;
        public PacketBuffer PacketBuf;
    }
}