using System.IO;
using System.Net;
using System.Net.Http;

namespace Morpheus.Network
{
    public class HttpResponse : IResponse
    {
        public ushort id;
        public HttpStatusCode statusCode;
        public Stream stream;

        public ushort Id
        {
            get => id;
            set => id = value;
        }

        public HttpResponse(HttpResponseMessage resp)
        {
            statusCode = resp.StatusCode;
            stream = resp.Content.ReadAsStreamAsync().Result;
        }

        public void Unpack(PacketBuffer source)
        {
            throw new System.NotImplementedException();
        }
    }
}