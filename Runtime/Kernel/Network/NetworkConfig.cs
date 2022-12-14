using System;
using UnityEngine;

namespace Morpheus.Network
{
    public class NetworkConfig : ScriptableObject
    {
        [NonSerialized] public IResponseProducer[] ResponseProducer;

        public SocketConfig[] SocketConfigs;

        private void Awake()
        {
            if (SocketConfigs != null && SocketConfigs.Length != 0)
            {
                return;
            }

            SocketConfig config = new SocketConfig
            {
                ConnectionConfig = new SocketConnectionConfig
                {
                    IpAdderss = NetworkUtil.GetLocalPrivateIp(),
                    Port = NetworkManager.DefalutPort
                }
            };
            SocketConfigs = new SocketConfig[] { config };
        }
    }
}