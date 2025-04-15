using Blanketmen.Hypnos.Mediation;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Blanketmen.Hypnos.Network
{
    public sealed partial class NetworkManager : EventDispatcher<int>
    {
        #region Singleton
        public static NetworkManager Instance { get; } = new NetworkManager();

        private NetworkManager() { }
        #endregion

        private ISocket[] sockets;
        private IClientSocket[] clients;
        private IServerSocket[] servers;

        private bool running;
        private Thread ioThread;
        private readonly IOContext ioContext = new IOContext();

        public void SetConfig(NetworkConfig cfg)
        {
            // TODO: Create sockets
        }

        public void Initialize()
        {
            // TODO: Need to check if the sockets are available.
            if (running)
            {
                Logging.Error("NetworkManager is already initialized.", nameof(NetworkManager));
                return;
            }

            running = true;
            ioThread = new Thread(ProcessIOEvents)
            {
                IsBackground = true,
                Name = "Network"
            };
            ioThread.Start();
        }

        public void Release()
        {
            if (!running)
            {
                Logging.Error("NetworkManager is not initialized.", nameof(NetworkManager));
                return;
            }

            running = false; // TODO: Use atomic.
            ioContext.signal.Set();
            ioThread.Join();

            foreach (ISocket socket in sockets)
            {
                socket?.Stop();
            }
        }

        public void Update()
        {
            foreach (ISocket socket in sockets)
            {
                socket.Dispatch();
            }
        }

        private void ProcessIOEvents()
        {
            while (running)
            {
                ioContext.signal.WaitOne();
                while (ioContext.events.TryDequeue(out IOEventArgs args))
                {
                    args.Process();
                }
                ioContext.signal.Reset();
            }
        }

        public void StartServer(uint id)
        {
            if (id >= servers.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            servers[id].Start();
        }

        public void StopServer(uint id)
        {
            if (id >= servers.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            servers[id].Stop();
        }

        public void StartClient(uint id)
        {
            if (id >= clients.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            clients[id].Start();
        }

        public void StopClient(uint id)
        {
            if (id >= clients.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            clients[id].Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(uint id, IResponse req, IEnumerable<ConnectionHandle> conns)
        {
            servers[id].Send(req, conns);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(uint id, IRequest req)
        {
            clients[id].Send(req);
        }

        public void Register(uint id, ServerEventHandler handler)
        {
            if (id >= servers.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            servers[id].Register(handler);
        }

        public void Unregister(uint id, ServerEventHandler handler)
        {
            if (id >= servers.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            servers[id].Unregister(handler);
        }

        public void Register(uint id, ClientEventHandler handler)
        {
            if (id >= clients.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            clients[id].Register(handler);
        }

        public void Unregister(uint id, ClientEventHandler handler)
        {
            if (id >= clients.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            clients[id].Unregister(handler);
        }

        public void Register(uint id, ushort gid, RequestHandler handler)
        {
            if (id >= servers.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            servers[id].Register(gid, handler);
        }

        public void Unregister(uint id, ushort gid, RequestHandler handler)
        {
            if (id >= servers.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            servers[id].Unregister(gid, handler);
        }

        public void Register(uint id, ushort gid, ResponseHandler handler)
        {
            if (id >= clients.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            clients[id].Register(gid, handler);
        }

        public void Unregister(uint id, ushort gid, ResponseHandler handler)
        {
            if (id >= clients.Length)
            {
                Logging.Error($"Invalid message id. MessageId: {id}", nameof(NetworkManager));
                return;
            }

            clients[id].Unregister(gid, handler);
        }
    }
}