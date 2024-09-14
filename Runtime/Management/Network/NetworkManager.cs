using Blanketmen.Hypnos.Mediation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Blanketmen.Hypnos
{
    public sealed partial class NetworkManager : EventDispatcher<int>
    {
        #region Singleton
        public static NetworkManager Instance { get; } = new NetworkManager();

        private NetworkManager() { }
        #endregion

        private readonly Dictionary<int, IConnection> connectionMap = new Dictionary<int, IConnection>(3);
        private readonly ConcurrentQueue<ConnectionEventArgs> socketEventArgs = new ConcurrentQueue<ConnectionEventArgs>();

        private IResponseProducer[] responseProducers;
        private readonly Dictionary<ushort, Action<IResponse>> responseHandlerMap = new Dictionary<ushort, Action<IResponse>>(521);

        public void Initialize(NetworkConfig config)
        {
            responseProducers = config.responseProducers;
            for (int i = 0; i < config.connectionConfigs.Length; ++i)
            {
                AddConnection(config.connectionConfigs[i]);
            }
        }

        public void Release()
        {
            foreach (IConnection conn in connectionMap.Values)
            {
                conn.Dispose();
            }
        }

        public void Update()
        {
            while (socketEventArgs.TryDequeue(out ConnectionEventArgs arg))
            {
                ProcessConnectionEventArg(arg);
            }

            // Dispatch responses
            foreach (IConnection conn in connectionMap.Values)
            {
                while (conn.TryGetResponse(out IResponse resp))
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

        public void AddConnection(ConnectionConfig config)
        {
            if (connectionMap.ContainsKey(config.id))
            {
                Logging.Error($"Connection is duplicate. ConnectionId: {config.id}", nameof(NetworkManager));
                return;
            }

            HandlerConfig handlerConfig = new HandlerConfig
            {
                onConnectionAoCompleteHandler = OnConnectionAoComplete,
                responseProducer = responseProducers[config.responseProducerId]
            };

            TransportConfig transportConfig = config.transportConfig;
            switch (transportConfig.protocol)
            {
                case TransportProtocol.LocalSimulation:
                {
                    connectionMap[config.id] = new LocalSocket(config.id, handlerConfig);
                    break;
                }
                case TransportProtocol.TCP:
                {
                    connectionMap[config.id] = new TcpSocket(config.id, transportConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.UDP:
                {
                    connectionMap[config.id] = new UdpSocket(config.id, transportConfig, handlerConfig);
                    break;
                }
                case TransportProtocol.HTTP:
                {
                    connectionMap[config.id] = new HttpConnection(config.id, handlerConfig);
                    break;
                }
                default:
                {
                    Logging.Error($"Protocol not implemented. ConnectionId: {config.id}, Protocol: {transportConfig.protocol}", nameof(NetworkManager));
                    break;
                }
            }
        }

        public void RemoveConnection(int id)
        {
            connectionMap.TryGetValue(id, out IConnection conn);
            if (conn == null)
            {
                return;
            }

            connectionMap.Remove(id);
            conn.Dispose();
        }

        public void ConnectAsync(int connectionId)
        {
            connectionMap.TryGetValue(connectionId, out IConnection conn);
            if (conn == null)
            {
                Logging.Info($"Can't find socket to connect. ConnectionId: {connectionId}", (int)LogChannel.Network);
                return;
            }

            conn.ConnectAsync();
        }

        public void DisconnectAsync(int connectionId)
        {
            connectionMap.TryGetValue(connectionId, out IConnection conn);
            if (conn == null)
            {
                Logging.Info($"Can't find socket to disconnect. ConnectionId: {connectionId}", (int)LogChannel.Network);
                return;
            }

            conn.DisconnectAsync();
        }

        public void SendRequest(int connectionId, IRequest request)
        {
            connectionMap.TryGetValue(connectionId, out IConnection conn);
            if (conn == null)
            {
                Logging.Info($"Can't find socket to send request. ConnectionId: {connectionId}", (int)LogChannel.Network);
                return;
            }

            conn.SendAsync(request);
        }

        // NOTE: May be called by multiple threads.
        private void OnConnectionAoComplete(IConnection conn, SocketAsyncOperation operation, SocketError socketError)
        {
            Logging.Info($"OnConnectionAoComplete. ConnectionId: {conn.Id}, Operation: {operation}, Error: {socketError}", (int)LogChannel.Network);
            ConnectionEventArgs args = new ConnectionEventArgs()
            {
                connection = conn,
                version = conn.Version,
                operation = operation,
                result = socketError
            };
            socketEventArgs.Enqueue(args);
        }

        // NOTE: Only called by main thread.
        private void ProcessConnectionEventArg(ConnectionEventArgs args)
        {
            if (args.version != args.connection.Version)
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
                            args.connection.ReceiveAsync();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.ConnectComplete, args.connection.Id, args.result);
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
                            args.connection.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.DisconnectComplete, args.connection.Id, args.result);
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
                            args.connection.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.ReceiveError, args.connection.Id);
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
                            args.connection.Reset();
                            break;
                        }
                    }
                    Notify((int)NetworkEvent.SendError, args.connection.Id);
                    return;
                }
            }
        }
    }
}