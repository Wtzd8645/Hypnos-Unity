using System;
using UnityEngine;

namespace Morpheus.Network
{
    public class NetworkConfig : ScriptableObject
    {
        public ConnectionConfig[] connectionConfigs;

        [NonSerialized] public IResponseProducer[] responseProducers;

        private void Awake()
        {
            if (connectionConfigs != null && connectionConfigs.Length != 0)
            {
                return;
            }

            ConnectionConfig config = new ConnectionConfig
            {
                socketConfig = new TransportConfig
                {
                    ip = NetworkUtil.GetLocalPrivateIp(),
                    port = NetworkManager.DefalutPort
                }
            };
            connectionConfigs = new ConnectionConfig[] { config };
        }
    }
}