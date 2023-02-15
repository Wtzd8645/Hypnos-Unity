using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Hypnos.Network
{
    public class HttpResponse : IResponse
    {
        public ushort id;
        public HttpStatusCode statusCode;
        public Stream stream;

        ushort IResponse.Id
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
            throw new NotImplementedException();
        }
    }
}