using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.TestTools;
using Blanketmen.Hypnos.Network;

namespace Blanketmen.Hypnos.Tests.Network
{
    internal class NetworkTest : MonoBehaviour, IMonoBehaviourTest
    {
        [UnityTest]
        public static IEnumerator TcpSocketPasses()
        {
            yield return new MonoBehaviourTest<NetworkTest>();
        }

        private uint connId = 0;
        private bool isConnected = false;
        private bool isSendFinished = false;
        private bool isReceiveFinished = false;
        private int totalRequestNum = 65535;
        private int receivedResponseNum = 0;

        public bool IsTestFinished { get; private set; } = false;

        public void Awake()
        {
            SocketConfig connCfg = new SocketConfig
            {
                id = connId,
                protocol = TransportProtocol.TCP,
                ip = "",
                port = 27015,
                responseAllocator = new TestResponseAllocator(),
            };
            NetworkConfig networkCfg = ScriptableObject.CreateInstance<NetworkConfig>();
            networkCfg.clientConfigs = new SocketConfig[] { connCfg };

            NetworkManager.Instance.SetConfig(networkCfg);
            NetworkManager.Instance.Initialize();
            //NetworkManager.Instance.Register<int, SocketError>((int)NetworkEventType.ConnectComplete, OnConnectComplete);
            NetworkManager.Instance.Register(0, (ushort)ResponseId.Echo, OnEchoResponse);
            NetworkManager.Instance.StartServer(connId);
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
            //NetworkManager.Instance.Unregister<int, SocketError>((int)NetworkEventType.ConnectComplete, OnConnectComplete);
            NetworkManager.Instance.Unregister(0, (ushort)ResponseId.Echo, OnEchoResponse);
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
                NetworkManager.Instance.Send(connId, req);
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