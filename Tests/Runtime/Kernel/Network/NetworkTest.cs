using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.TestTools;

namespace Blanketmen.Hypnos.Tests.Network
{
    internal class NetworkTest : MonoBehaviour, IMonoBehaviourTest
    {
        [UnityTest]
        public static IEnumerator TcpConnectionPasses()
        {
            yield return new MonoBehaviourTest<NetworkTest>();
        }

        private int connId = 0;
        private bool isConnected = false;
        private bool isSendFinished = false;
        private bool isReceiveFinished = false;
        private int totalRequestNum = 65535;
        private int receivedResponseNum = 0;

        public bool IsTestFinished { get; private set; } = false;

        public void Awake()
        {
            TransportConfig transCfg = new TransportConfig
            {
                protocol = TransportProtocol.TCP,
                ip = "", // TODO: Get server IP.
                port = NetworkManager.DefalutPort,
                maxPacketSize = 1024
            };
            ConnectionConfig connCfg = new ConnectionConfig
            {
                id = connId,
                transportConfig = transCfg,
                responseProducerId = 0
            };
            NetworkConfig networkCfg = ScriptableObject.CreateInstance<NetworkConfig>();
            networkCfg.connectionConfigs = new ConnectionConfig[] { connCfg };
            networkCfg.responseProducers = new IResponseProducer[] { new ResponseProducer() };

            NetworkManager.CreateInstance();
            NetworkManager.Instance.Initialize(networkCfg);
            NetworkManager.Instance.Register<int, SocketError>((int)NetworkEvent.ConnectComplete, OnConnectComplete);
            NetworkManager.Instance.Register((ushort)ResponseId.Echo, OnEchoResponse);
            NetworkManager.Instance.ConnectAsync(connId);
        }

        private void Update()
        {
            NetworkManager.Instance.Update();
            if (isConnected && isSendFinished && isReceiveFinished)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            NetworkManager.Instance.Unregister<int, SocketError>((int)NetworkEvent.ConnectComplete, OnConnectComplete);
            NetworkManager.Instance.Unregister((ushort)ResponseId.Echo, OnEchoResponse);
            NetworkManager.ReleaseInstance();
        }

        private void OnConnectComplete(int id, SocketError result)
        {
            isConnected = result == SocketError.Success;
            if (!isConnected)
            {
                IsTestFinished = true;
                return;
            }

            SendRequests();
        }

        private void SendRequests()
        {
            if (!isConnected || isSendFinished)
            {
                return;
            }

            EchoRequest req = new EchoRequest
            {
                a = 0,
                b = "Trinity",
                c = 3.1415926d
            };

            for (int i = 0; i < totalRequestNum; ++i)
            {
                NetworkManager.Instance.SendRequest(connId, req);
                ++req.a;
            }
            isSendFinished = true;
        }

        private void OnEchoResponse(IResponse response)
        {
            EchoResponse echoResp = response as EchoResponse;
            if (echoResp.a != receivedResponseNum)
            {
                isReceiveFinished = true;
            }
            ++receivedResponseNum;
        }
    }
}