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
            responseProducers = config.ResponseProducer;
            for (int i = 0; i < config.SocketConfigs.Length; ++i)
            {
                AddSocket(config.SocketConfigs[i]);
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
                    Logger.LogWarning($"[NetworkManager] Response handler is null. MsgId: {response.Id}");
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
            if (socketMap.ContainsKey(config.Id))
            {
                Logger.LogError($"[NetworkManager] Socket is duplicate. SocketId: {config.Id}");
                return;
            }

            SocketHandlerConfig handlerConfig = new SocketHandlerConfig
            {
                OnSocketAoCompleteHandler = OnSocketAoComplete,
                ResponseProducer = responseProducers[config.ResponseProducerId],
                OnResponseCompleteHandler = OnResponseComplete

            };

            SocketConnectionConfig connConfig = config.ConnectionConfig;
            switch (connConfig.Protocol)
            {
                case TransportProtocol.LocalSimulation:
                {
                    socketMap[config.Id] = new LocalSocket(config.Id, handlerConfig);
                    break;
                }
                case TransportProtocol.Tcp:
                {
                    socketMap[config.Id] = new TcpSocket(config.Id, connConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.Udp:
                {
                    socketMap[config.Id] = new UdpSocket(config.Id, connConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.Http:
                {
                    socketMap[config.Id] = new HttpSocket(config.Id, handlerConfig);
                    break;
                }
                default:
                {
                    Logger.LogError($"[NetworkManager] Protocol not implemented. SocketId: {config.Id}, Protocol: {connConfig.Protocol}");
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
                Logger.Log($"[NetworkManager] Can't find socket to connect. SocketId: {socketId}", (int)DebugLogChannel.Network);
                return;
            }

            socket.ConnectAsync();
        }

        public void DisconnectAsync(int socketId)
        {
            socketMap.TryGetValue(socketId, out ISocket socket);
            if (socket == null)
            {
                Logger.Log($"[NetworkManager] Can't find socket to disconnect. SocketId: {socketId}", (int)DebugLogChannel.Network);
                return;
            }

            socket.DisconnectAsync();
        }

        public void SendRequest(int socketId, IRequest request)
        {
            socketMap.TryGetValue(socketId, out ISocket socket);
            if (socket == null)
            {
                Logger.Log($"[NetworkManager] Can't find socket to send request. SocketId: {socketId}", (int)DebugLogChannel.Network);
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
            Logger.Log($"[NetworkManager] OnStreamSocketAoComplete. SocketId: {socket.Id}, Operation: {operation}, Error: {socketError}", (int)DebugLogChannel.Network);
            SocketEventArgs args = new SocketEventArgs()
            {
                Socket = socket,
                SocketVersion = socket.Version,
                Operation = operation,
                Result = socketError
            };
            socketEventArgs.Enqueue(args);
        }

        // NOTE: Only called by main thread.
        private void ProcessSocketEventArg(SocketEventArgs args)
        {
            if (args.SocketVersion != args.Socket.Version)
            {
                return;
            }

            switch (args.Operation)
            {
                case SocketAsyncOperation.Connect:
                {
                    switch (args.Result)
                    {
                        case SocketError.IsConnected:
                        {
                            break;
                        }
                        case SocketError.Success:
                        {
                            args.Socket.ReceiveAsync();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.ConnectComplete, args.Socket.Id, args.Result);
                    return;
                }
                case SocketAsyncOperation.Disconnect:
                {
                    switch (args.Result)
                    {
                        case SocketError.NotConnected:
                        {
                            break;
                        }
                        case SocketError.Success:
                        {
                            args.Socket.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.DisconnectComplete, args.Socket.Id, args.Result);
                    return;
                }
                case SocketAsyncOperation.Receive:
                {
                    switch (args.Result)
                    {
                        case SocketError.OperationAborted:
                        case SocketError.NetworkReset:
                        case SocketError.ConnectionReset:
                        {
                            break;
                        }
                        default:
                        {
                            args.Socket.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.ReceiveError, args.Socket.Id);
                    return;
                }
                case SocketAsyncOperation.Send:
                {
                    switch (args.Result)
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
                            args.Socket.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.SendError, args.Socket.Id);
                    return;
                }
            }
        }
    }
}