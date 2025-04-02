using System.Collections.Generic;
using System.Net.Sockets;

namespace Blanketmen.Hypnos.Network
{
    public delegate void SocketAoHandler(ISocket conn, SocketAsyncOperation op, SocketError err);

    public static class NetworkDefs
    {
        public const int MAX_ETH_MTU = 1500;  // Ethernet (Standard) MTU.
        public const int MAX_WIFI_MTU = 1500;  // Wi-Fi (802.11) MTU.
        public const int MAX_PPPoE_MTU = 1492;  // PPPoE (DSL) MTU.
        public const int MAX_VPN_MTU = 1476;  // VPN (GRE Tunnel) MTU.
        public const int MAX_JUMBO_MTU = 9000;  // Jumbo Frames MTU.
        public const int MAX_LOOPBACK_MTU = 65536;  // Loopback (lo Interface) MTU.
        public const int MIN_IPV6_MTU = 1280;  // IPv6 (Minimum) MTU.

        public const int MAX_PACKET_SIZE = MAX_VPN_MTU;
        public const int MAX_BUFFER_SIZE = 2048;

        public const int PACKET_SIZE_LENGTH = sizeof(ushort);
        public const int MESSAGE_ID_LENGTH = sizeof(ushort);
        public const int DEFAULT_SEND_TIMEOUT = 4096;

        public static readonly System.Text.Encoding StringEncoder = new System.Text.UTF8Encoding(false, true);
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

    internal class SocketEventArgs
    {
        public ISocket socket;
        public uint version;
        public SocketAsyncOperation op;
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