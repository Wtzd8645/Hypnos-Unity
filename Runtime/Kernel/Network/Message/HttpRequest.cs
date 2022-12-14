using System.Net.Http;

namespace Morpheus.Network
{
    public class HttpRequest : IRequest
    {
        public ushort id;
        public string uri;
        public HttpMethod method;
        public HttpContent content;

        public ushort Id => id;

        public int Pack(PacketBuffer result)
        {
            throw new System.NotImplementedException();
        }
    }
}