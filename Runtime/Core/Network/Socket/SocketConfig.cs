using System;
using UnityEngine;

namespace Blanketmen.Hypnos.Network
{
    [Serializable]
    public class SocketConfig
    {
        public uint id;
        public TransportProtocol protocol;
        public string ip;
        public int port = 27015;

        [SerializeReference, SelectableField] public IRequestAllocator requestAllocator;
        [SerializeReference, SelectableField] public IResponseAllocator responseAllocator;
        public int sendTimeout = NetworkDefs.DEFAULT_SEND_TIMEOUT;
    }
}