using Morpheus.Core.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Morpheus.Network
{
    public sealed partial class NetworkManager : Subject<int>
    {
        #region Singleton
        public static NetworkManager Instance { get; private set; }

        internal static void CreateInstance()
        {
            Instance ??= new NetworkManager();
        }

        internal static void ReleaseInstance()
        {
            Instance = null;
        }

        private NetworkManager() { }
        #endregion

        private readonly Dictionary<int, ISocket> socketMap = new Dictionary<int, ISocket>(3);
        private readonly ConcurrentQueue<SocketEventArgs> socketEventArgs = new ConcurrentQueue<SocketEventArgs>();

        private IResponseProducer[] responseProducers;
        private readonly ConcurrentQueue<IResponse> pendingResponses = new ConcurrentQueue<IResponse>(); // NOTE: Only produced from the receiving thread.
        private readonly Dictionary<ushort, Action<IResponse>> responseHandlerMap = new Dictionary<ushort, Action<IResponse>>(521);

        public void Initialize(NetworkConfig config)
        {
            responseProducers = config.responseProducer;
            for (int i = 0; i < config.socketConfigs.Length; ++i)
            {
                AddSocket(config.socketConfigs[i]);
            }
        }

        public void Release()
        {
            foreach (ISocket socket in socketMap.Values)
            {
                socket.Dispose();
            }
        }

        internal void Update()
        {
            while (socketEventArgs.TryDequeue(out SocketEventArgs arg))
            {
                ProcessSocketEventArg(arg);
            }

            // Dispatch responses
            while (pendingResponses.TryDequeue(out IResponse response))
            {
                responseHandlerMap.TryGetValue(response.Id, out Action<IResponse> responseHandler);
                if (responseHandler == null)
                {
                    DebugLogger.LogWarning($"[NetworkManager] Response handler is null. MsgId: {response.Id}");
                    continue;
                }
                responseHandler(response);
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

        public void AddSocket(SocketConfig config)
        {
            if (socketMap.ContainsKey(config.id))
            {
                DebugLogger.LogError($"[NetworkManager] Socket is duplicate. SocketId: {config.id}");
                return;
            }

            SocketHandlerConfig handlerConfig = new SocketHandlerConfig
            {
                onSocketAoCompleteHandler = OnSocketAoComplete,
                responseProducer = responseProducers[config.responseProducerId],
                onResponseCompleteHandler = OnResponseComplete

            };

            SocketConnectionConfig connConfig = config.connectionConfig;
            switch (connConfig.protocol)
            {
                case TransportProtocol.LocalSimulation:
                {
                    socketMap[config.id] = new LocalSocket(config.id, handlerConfig);
                    break;
                }
                case TransportProtocol.TCP:
                {
                    socketMap[config.id] = new TcpSocket(config.id, connConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.UDP:
                {
                    socketMap[config.id] = new UdpSocket(config.id, connConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.HTTP:
                {
                    socketMap[config.id] = new HttpSocket(config.id, handlerConfig);
                    break;
                }
                default:
                {
                    DebugLogger.LogError($"[NetworkManager] Protocol not implemented. SocketId: {config.id}, Protocol: {connConfig.protocol}");
                    break;
                }
            }
        }

        public void RemoveSocket(int id)
        {
            socketMap.TryGetValue(id, out ISocket socket);
            if (socket == null)
            {
                return;
            }

            socketMap.Remove(id);
            socket.Dispose();
        }

        public void ConnectAsync(int socketId)
        {
            socketMap.TryGetValue(socketId, out ISocket socket);
            if (socket == null)
            {
                DebugLogger.Log($"[NetworkManager] Can't find socket to connect. SocketId: {socketId}", (int)DebugLogChannel.Network);
                return;
            }

            socket.ConnectAsync();
        }

        public void DisconnectAsync(int socketId)
        {
            socketMap.TryGetValue(socketId, out ISocket socket);
            if (socket == null)
            {
                DebugLogger.Log($"[NetworkManager] Can't find socket to disconnect. SocketId: {socketId}", (int)DebugLogChannel.Network);
                return;
            }

            socket.DisconnectAsync();
        }

        public void SendRequest(int socketId, IRequest request)
        {
            socketMap.TryGetValue(socketId, out ISocket socket);
            if (socket == null)
            {
                DebugLogger.Log($"[NetworkManager] Can't find socket to send request. SocketId: {socketId}", (int)DebugLogChannel.Network);
                return;
            }

            socket.SendAsync(request);
        }

        private void OnResponseComplete(ISocket socket, IResponse resp)
        {
            pendingResponses.Enqueue(resp);
        }

        // NOTE: May be called by multiple threads.
        private void OnSocketAoComplete(SocketBase socket, SocketAsyncOperation operation, SocketError socketError)
        {
            DebugLogger.Log($"[NetworkManager] OnStreamSocketAoComplete. SocketId: {socket.id}, Operation: {operation}, Error: {socketError}", (int)DebugLogChannel.Network);
            SocketEventArgs args = new SocketEventArgs()
            {
                socket = socket,
                socketVersion = socket.version,
                operation = operation,
                result = socketError
            };
            socketEventArgs.Enqueue(args);
        }

        // NOTE: Only called by main thread.
        private void ProcessSocketEventArg(SocketEventArgs args)
        {
            if (args.socketVersion != args.socket.version)
            {
                return;
            }

            switch (args.operation)
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
                    Notify((int)NetworkEvent.ConnectComplete, args.socket.id, args.result);
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
                    Notify((int)NetworkEvent.DisconnectComplete, args.socket.id, args.result);
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
                    Notify((int)NetworkEvent.ReceiveError, args.socket.id);
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
                    Notify((int)NetworkEvent.SendError, args.socket.id);
                    return;
                }
            }
        }
    }
}