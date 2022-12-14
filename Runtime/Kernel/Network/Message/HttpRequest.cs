using System.Net.Http;

namespace Morpheus.Network
{
    public class HttpRequest : IRequest
    {
        public ushort RequestId;
        public string Uri;
        public HttpMethod Method;
        public HttpContent Content;

        public ushort Id => RequestId;

        public int Pack(PacketBuffer result)
        {
            return 0;
        }
    }
}