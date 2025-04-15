using System.Collections.Generic;

namespace Blanketmen.Hypnos.Network
{
    internal interface IServerSocket : ISocket
    {
        public void Send(IResponse resp, IEnumerable<ConnectionHandle> events);

        public void Register(ServerEventHandler handler);
        public void Unregister(ServerEventHandler handler);

        public void Register(ushort gid, RequestHandler handler);
        public void Unregister(ushort gid, RequestHandler handler);
    }
}