using System;
using System.Net.Http;

namespace Blanketmen.Hypnos.Network
{
    public class HttpRequest : IRequest
    {
        public ushort id;
        public string uri;
        public HttpMethod method;
        public HttpContent content;

        public ushort Id => id;

        public ushort Pack(Span<byte> buf)
        {
            throw new NotImplementedException();
        }
    }
}