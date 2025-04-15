using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blanketmen.Hypnos.Network
{
    public enum HttpMethod
    {
        Connect,
        Head,
        Get,
        Post,
        Put,
        Delete,
        Options,
        Trace
    }

    internal class HttpClientSocketAdap : IClientSocket
    {
        private HttpClient client = new HttpClient();

        public uint Id { get; private set; }

        public uint Version { get; private set; }

        public HttpClientSocketAdap(SocketConfig cfg)
        {
            Id = cfg.id;
            //onConnectionAoComplete = packetCfg.onSocketAoCompleteHandler;
            //responseProducer = packetCfg.responseAlloc;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public void Reset()
        {
            client.Dispose();
            client = new HttpClient();
            ++Version;
        }

        public void SetAddress(string ip, int port) { }

        public void Start() { }

        public void Stop() { }

        public void Dispatch()
        {

        }

        public void Send(IRequest request)
        {
            SendAsyncInternal(request as HttpRequest);
        }

        private async void SendAsyncInternal(HttpRequest request)
        {
            try
            {
                HttpResponseMessage msg;
                switch (request.method)
                {
                    case HttpMethod.Get:
                    {
                        msg = await client.GetAsync(request.uri);
                        break;
                    }
                    case HttpMethod.Post:
                    {
                        msg = await client.PostAsync(request.uri, request.content);
                        break;
                    }
                    default:
                    {
                        return;
                    }
                }

                msg.EnsureSuccessStatusCode();
                //responses.Enqueue(new HttpResponse(msg));
            }
            catch (HttpRequestException e)
            {
                Logging.Error($"Send HttpRequest exception. Exception: {e.Message}", nameof(HttpClientSocketAdap));
                //onConnectionAoComplete(Id, SocketAsyncOperation.Send, SocketError.Fault);
            }
            catch (TaskCanceledException e)
            {
                Logging.Error($"Send HttpRequest exception. Exception: {e.Message}", nameof(HttpClientSocketAdap));
                //onConnectionAoComplete(Id, SocketAsyncOperation.Send, SocketError.OperationAborted);
            }
            catch (Exception e)
            {
                Logging.Error($"Send HttpRequest exception. Exception: {e.Message}", nameof(HttpClientSocketAdap));
                //onConnectionAoComplete(Id, SocketAsyncOperation.Send, SocketError.SocketError);
            }
        }

        public void Register(ClientEventHandler handler)
        {

        }

        public void Unregister(ClientEventHandler handler)
        {

        }

        public void Register(ushort gid, ResponseHandler handler)
        {

        }

        public void Unregister(ushort gid, ResponseHandler handler)
        {

        }

        public void Process(IOEventArgs args)
        {

        }
    }
}