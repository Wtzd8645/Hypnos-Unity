using System.IO;
using System.Net;
using System.Net.Http;

namespace Morpheus.Network
{
    public class HttpResponse : IResponse
    {
        public ushort ResponseId;
        public HttpStatusCode StatusCode;
        public Stream Stream;

        public ushort Id
        {
            get => ResponseId;
            set => ResponseId = value;
        }

        public HttpResponse(HttpResponseMessage resp)
        {
            StatusCode = resp.StatusCode;
            Stream = resp.Content.ReadAsStreamAsync().Result;
        }

        public void Unpack(PacketBuffer source)
        {
            throw new System.NotImplementedException();
        }
    }
}