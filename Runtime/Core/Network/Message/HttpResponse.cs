using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Blanketmen.Hypnos.Network
{
    public class HttpResponse : IResponse
    {
        public HttpStatusCode statusCode;
        public Stream stream;

        public byte Gid { get; set; }
        public ushort Id { get; set; }

        public HttpResponse(HttpResponseMessage resp)
        {
            statusCode = resp.StatusCode;
            stream = resp.Content.ReadAsStreamAsync().Result;
        }

        public void Unpack(Span<byte> buf)
        {

        }
    }
}