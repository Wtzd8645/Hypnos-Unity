using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Morpheus.Network
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

    internal class HttpSocket : ISocket
    {
        private HttpClient client = new HttpClient();
        private readonly IResponseProducer responseProducer;
        private readonly SocketResponseHandler onResponseComplete;

        public int Id { get; private set; }

        public HttpSocket(int id, SocketHandlerConfig handlerConfig)
        {
            Id = id;
            responseProducer = handlerConfig.ResponseProducer;
            onResponseComplete = handlerConfig.OnResponseCompleteHandler;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public void Reset()
        {
            client.Dispose();
            client = new HttpClient();
        }

        public void ConnectAsync() { }
        public void DisconnectAsync() { }
        public void ReceiveAsync() { }

        public void SendAsync(IRequest request)
        {
            Task.Factory.StartNew(SendAsyncInteral, request);
        }

        private async void SendAsyncInteral(object state)
        {
            try
            {
                HttpRequest request = state as HttpRequest;
                HttpResponseMessage response;
                switch (request.Method)
                {
                    case HttpMethod.Get:
                    {
                        response = await client.GetAsync(request.Uri);
                        break;
                    }
                    case HttpMethod.Post:
                    {
                        response = await client.PostAsync(request.Uri, request.Content);
                        break;
                    }
                    default:
                    {
                        return;
                    }
                }

                response.EnsureSuccessStatusCode();
                onResponseComplete(this, responseProducer.Produce(response));
            }
            catch (HttpRequestException e)
            {
                Kernel.LogError(e.Message);
            }
            catch (TaskCanceledException e)
            {
                Kernel.LogError(e.Message);
            }
            catch (Exception e)
            {
                Kernel.LogError(e.Message);
            }
        }
    }
}