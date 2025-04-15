using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Blanketmen.Hypnos.Network
{
    internal abstract class ClientSocketBase : IClientSocket
    {
        protected uint id;
        protected Socket socket;
        protected uint version;

        protected EndPoint endPoint;
        protected readonly IOContext ioContext;
        protected readonly ConcurrentQueue<ClientEvent> socketEvents = new ConcurrentQueue<ClientEvent>();
        protected ClientEventHandler onSocketEvent; // NOTE: When ReceiveAsync & SendAsync successfully will not send events.

        protected readonly ConcurrentQueue<RequestEventArgs> requestArgs = new ConcurrentQueue<RequestEventArgs>();
        protected readonly IResponseAllocator responseAlloctor;
        protected readonly ConcurrentQueue<IResponse> responses = new ConcurrentQueue<IResponse>();
        protected readonly ResponseHandler[] responseHandlers;

        public uint Id => id;
        public uint Version => version;

        protected ClientSocketBase(SocketConfig cfg)
        {
            id = cfg.id;
            SetAddress(cfg.ip, cfg.port);
        }

        public void SetAddress(string ip, int port)
        {
            if (socket == null && IPAddress.TryParse(ip, out IPAddress ipa))
            {
                endPoint = new IPEndPoint(ipa, port);
            }
        }

        public abstract void Start();
        public abstract void Stop();

        public void Dispatch()
        {
            while (socketEvents.TryDequeue(out ClientEvent evt))
            {
                onSocketEvent?.Invoke(evt);
            }

            while (responses.TryDequeue(out IResponse resp))
            {
                responseHandlers[resp.Gid]?.Invoke(resp);
                responseAlloctor.Release(resp);
            }
        }

        public abstract void Send(IRequest request);

        public void Register(ClientEventHandler handler)
        {
            if (handler != null)
            {
                onSocketEvent += handler;
            }
        }

        public void Unregister(ClientEventHandler handler)
        {
            if (handler != null)
            {
                onSocketEvent -= handler;
            }
        }

        public void Register(ushort gid, ResponseHandler handler)
        {
            if (gid < responseHandlers.Length)
            {
                responseHandlers[gid] += handler;
            }
        }

        public void Unregister(ushort gid, ResponseHandler handler)
        {
            if (gid < responseHandlers.Length)
            {
                responseHandlers[gid] -= handler;
            }
        }

        public abstract void Process(IOEventArgs evt);

        protected void CloseInternal(SocketAsyncOperation op, SocketError err)
        {
            if (socket == null)
            {
                return;
            }

            Logging.Info($"Close socket. Op: {op}, Error: {err}", (int)LogChannel.Network);
            socket.Dispose(); // NOTE: https://docs.microsoft.com/zh-tw/dotnet/api/system.net.sockets.socket.close
            socket = null;
            version++;

            requestArgs.Clear();
            OnSocketEvent(ClientEvent.Type.Disconnect, err);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnSocketEvent(ClientEvent.Type op, SocketError err)
        {
            socketEvents.Enqueue(new ClientEvent
            {
                type = op,
                error = err
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnIOComplete(object sender, SocketAsyncEventArgs args)
        {
            ioContext.events.Enqueue(args.UserToken as IOEventArgs);
            ioContext.signal.Set();
        }
    }
}