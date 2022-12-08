using System;
using UnityEngine;

namespace Morpheus.Network
{
    public class NetworkConfig : ScriptableObject
    {
        [NonSerialized] public IResponseProducer[] responseProducer;

        public SocketConfig[] socketConfigs;

        private void Awake()
        {
            if (socketConfigs != null && socketConfigs.Length != 0)
            {
                return;
            }

            SocketConfig config = new SocketConfig
            {
                connectionConfig = new SocketConnectionConfig
                {
                    ipAdderss = NetworkUtil.GetLocalPrivateIp(),
                    port = NetworkManager.DefalutPort
                }
            };
            socketConfigs = new SocketConfig[] { config };
        }
    }
}