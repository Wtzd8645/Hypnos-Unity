using System;
using UnityEngine;

namespace Blanketmen.Hypnos.Network
{
    public class NetworkConfig : ScriptableObject
    {
        public SocketConfig[] socketConfigs;

        [NonSerialized] public IResponseProducer[] responseProducers;

        private void Awake()
        {
            if (socketConfigs != null && socketConfigs.Length != 0)
            {
                return;
            }

            SocketConfig config = new SocketConfig
            {
                transportConfig = new TransportConfig
                {
                    ip = NetworkUtils.GetLocalPrivateIp(),
                    port = 27015
                }
            };
            socketConfigs = new SocketConfig[] { config };
        }
    }
}