using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Blanketmen.Hypnos.Network
{
    // SocketError: https://docs.microsoft.com/zh-tw/windows/win32/winsock/windows-sockets-error-codes-2

    public delegate void ServerEventHandler(ServerEvent evt);
    public delegate void ClientEventHandler(ClientEvent evt);
    public delegate void RequestHandler(IRequest req);
    public delegate void ResponseHandler(IResponse resp);

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

    public struct ServerEvent
    {
        public enum Type
        {
            Connect,
            Disconnect,
        }

        public Type type;
        public SocketError error;
        public ConnectionHandle handle;
    }

    public struct ClientEvent
    {
        public enum Type
        {
            Connect,
            Disconnect
        }

        public Type type;
        public SocketError error;
    }

    internal struct SocketHandle
    {
        public ISocket sock;
        public uint version;

        public readonly bool IsValid() => sock != null && sock.Version == version;
    }

    internal class IOEventArgs
    {
        public enum Operation
        {
            None,
            Accept,
            Connect,
            Receive,
            Send
        }

        public SocketHandle sockHandle;
        public ConnectionHandle connHandle;
        public Operation op;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process()
        {
            if (sockHandle.IsValid())
            {
                sockHandle.sock.Process(this);
            }
        }
    }

    internal class IOContext
    {
        public ManualResetEvent signal = new ManualResetEvent(false);
        public ConcurrentQueue<IOEventArgs> events = new ConcurrentQueue<IOEventArgs>();
    }

    internal struct RequestEventArgs
    {
        public byte[] buffer;
        public ushort length;
    }

    internal struct ResponseEventArg
    {
        public List<ConnectionHandle> conns;
        public byte[] buffer;
        public ushort length;
    }
}