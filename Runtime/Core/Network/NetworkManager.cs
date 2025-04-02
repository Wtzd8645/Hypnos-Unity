using Blanketmen.Hypnos.Mediation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Blanketmen.Hypnos.Network
{
    public sealed partial class NetworkManager : EventDispatcher<int>
    {
        #region Singleton
        public static NetworkManager Instance { get; } = new NetworkManager();

        private NetworkManager() { }
        #endregion

        private ISocket[] sockets;
        private readonly ConcurrentQueue<SocketEventArgs> socketEventArgs = new ConcurrentQueue<SocketEventArgs>();

        private IResponseProducer[] responseProducers;
        private readonly Dictionary<ushort, Action<IResponse>> responseHandlerMap = new Dictionary<ushort, Action<IResponse>>(521);

        public void Initialize(NetworkConfig config)
        {
            sockets = new ISocket[config.socketConfigs.Length];
            responseProducers = config.responseProducers;
            for (int i = 0; i < config.socketConfigs.Length; ++i)
            {
                AddConnection(config.socketConfigs[i]);
            }
        }

        public void Release()
        {
            foreach (ISocket socket in sockets)
            {
                socket.Dispose();
            }
        }

        public void Update()
        {
            while (socketEventArgs.TryDequeue(out SocketEventArgs arg))
            {
                ProcessSocketEventArg(arg);
            }

            // Dispatch responses
            foreach (ISocket socket in sockets)
            {
                while (socket.TryGetResponse(out IResponse resp))
                {
                    responseHandlerMap.TryGetValue(resp.Id, out Action<IResponse> responseHandler);
                    if (responseHandler == null)
                    {
                        Logging.Warning($"[NetworkManager] Response handler is null. MsgId: {resp.Id}");
                    }
                    else
                    {
                        responseHandler(resp);
                    }
                }
            }
        }

        public void Register(ushort msgId, Action<IResponse> handler)
        {
            responseHandlerMap.TryGetValue(msgId, out Action<IResponse> handlers);
            responseHandlerMap[msgId] = handlers + handler;
        }

        public void Unregister(ushort msgId, Action<IResponse> handler)
        {
            if (responseHandlerMap.TryGetValue(msgId, out Action<IResponse> handlers))
            {
                responseHandlerMap[msgId] = handlers - handler;
            }
        }

        public void AddConnection(SocketConfig config)
        {
            if (config.id >= sockets.Length)
            {
                Logging.Error($"Connection is duplicate. ConnectionId: {config.id}", nameof(NetworkManager));
                return;
            }

            HandlerConfig handlerConfig = new HandlerConfig
            {
                onSocketAoCompleteHandler = OnSocketAoComplete,
                responseProducer = responseProducers[config.responseProducerId]
            };

            TransportConfig transportConfig = config.transportConfig;
            switch (transportConfig.protocol)
            {
                case TransportProtocol.LocalSimulation:
                {
                    sockets[config.id] = new MockSocket(config.id, handlerConfig);
                    break;
                }
                case TransportProtocol.TCP:
                {
                    sockets[config.id] = new TcpSocket(config.id, transportConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.UDP:
                {
                    sockets[config.id] = new UdpSocket(config.id, transportConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.HTTP:
                {
                    sockets[config.id] = new HttpSocketAdap(config.id, handlerConfig);
                    break;
                }
                default:
                {
                    Logging.Error($"Protocol not implemented. SocketId: {config.id}, Protocol: {transportConfig.protocol}", nameof(NetworkManager));
                    break;
                }
            }
        }

        public void RemoveConnection(uint id)
        {
            if (id >= sockets.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            sockets[id]?.Dispose();
        }

        public void ConnectAsync(uint id)
        {
            if (id >= sockets.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            sockets[id].ConnectAsync();
        }

        public void DisconnectAsync(uint id)
        {
            if (id >= sockets.Length)
            {
                Logging.Error($"Invalid socket id. SocketId: {id}", nameof(NetworkManager));
                return;
            }

            sockets[id].DisconnectAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(uint Id, IRequest request)
        {
            sockets[Id].SendAsync(request);
        }

        // NOTE: May be called by multiple threads.
        private void OnSocketAoComplete(ISocket conn, SocketAsyncOperation op, SocketError err)
        {
            Logging.Info($"OnSocketAoComplete. SocketId: {conn.Id}, Operation: {op}, Error: {err}", (int)LogChannel.Network);
            SocketEventArgs args = new SocketEventArgs()
            {
                socket = conn,
                version = conn.Version,
                op = op,
                result = err
            };
            socketEventArgs.Enqueue(args);
        }

        // NOTE: Only called by main thread.
        private void ProcessSocketEventArg(SocketEventArgs args)
        {
            if (args.version != args.socket.Version)
            {
                return;
            }

            switch (args.op)
            {
                case SocketAsyncOperation.Connect:
                {
                    switch (args.result)
                    {
                        case SocketError.IsConnected:
                        {
                            break;
                        }
                        case SocketError.Success:
                        {
                            args.socket.ReceiveAsync();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.ConnectComplete, args.socket.Id, args.result);
                    return;
                }
                case SocketAsyncOperation.Disconnect:
                {
                    switch (args.result)
                    {
                        case SocketError.NotConnected:
                        {
                            break;
                        }
                        case SocketError.Success:
                        {
                            args.socket.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.DisconnectComplete, args.socket.Id, args.result);
                    return;
                }
                case SocketAsyncOperation.Receive:
                {
                    switch (args.result)
                    {
                        case SocketError.OperationAborted:
                        case SocketError.NetworkReset:
                        case SocketError.ConnectionReset:
                        {
                            break;
                        }
                        default:
                        {
                            args.socket.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.ReceiveError, args.socket.Id);
                    return;
                }
                case SocketAsyncOperation.Send:
                {
                    switch (args.result)
                    {
                        case SocketError.OperationAborted:
                        case SocketError.NetworkReset:
                        case SocketError.ConnectionReset:
                        {
                            return;
                        }
                        case SocketError.NoBufferSpaceAvailable:
                        case SocketError.TimedOut:
                        {
                            args.socket.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.SendError, args.socket.Id);
                    return;
                }
            }
        }
    }
}