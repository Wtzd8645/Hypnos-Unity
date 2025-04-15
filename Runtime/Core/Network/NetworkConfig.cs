using UnityEngine;

namespace Blanketmen.Hypnos.Network
{
    public class NetworkConfig : ScriptableObject
    {
        public bool useNetworkThread = true;
        public SocketConfig[] serverConfigs;
        public SocketConfig[] clientConfigs;

        private void Awake()
        {
            if (clientConfigs != null && clientConfigs.Length != 0)
            {
                return;
            }

            SocketConfig config = new SocketConfig
            {
                ip = NetworkUtils.GetLocalPrivateIp(),
                port = 27015,
            };
            clientConfigs = new SocketConfig[] { config };
        }
    }
}