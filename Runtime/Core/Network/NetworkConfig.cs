using System;
using UnityEngine;

namespace Blanketmen.Hypnos.Network
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
                transportConfig = new TransportConfig
                {
                    ip = NetworkUtils.GetLocalPrivateIp(),
                    port = 27015
                }
            };
            connectionConfigs = new ConnectionConfig[] { config };
        }
    }
}