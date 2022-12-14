using System;
using System.Collections.Concurrent;
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

    internal class HttpConnection : IConnection
    {
        private HttpClient client = new HttpClient();
        private readonly ConnectionAoHandler onConnectionAoComplete;
        private readonly IResponseProducer responseProducer;
        private readonly ConcurrentQueue<IResponse> pendingResponses = new ConcurrentQueue<IResponse>();

        public int Id { get; private set; }

        public int Version { get; private set; }

        public HttpConnection(int id, ConnectionHandlerConfig handlerConfig)
        {
            Id = id;
            onConnectionAoComplete = handlerConfig.onConnectionAoCompleteHandler;
            responseProducer = handlerConfig.responseProducer;
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

        public void ConnectAsync() { }

        public void DisconnectAsync() { }

        public void ReceiveAsync() { }

        public bool TryGetResponse(out IResponse response)
        {
            return pendingResponses.TryDequeue(out response);
        }

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
                switch (request.method)
                {
                    case HttpMethod.Get:
                    {
                        response = await client.GetAsync(request.uri);
                        break;
                    }
                    case HttpMethod.Post:
                    {
                        response = await client.PostAsync(request.uri, request.content);
                        break;
                    }
                    default:
                    {
                        return;
                    }
                }

                response.EnsureSuccessStatusCode();
                pendingResponses.Enqueue(responseProducer.Produce(response));
            }
            catch (HttpRequestException e)
            {
                Kernel.LogError(e.Message);
                onConnectionAoComplete(this, System.Net.Sockets.SocketAsyncOperation.Send, System.Net.Sockets.SocketError.Fault);
            }
            catch (TaskCanceledException e)
            {
                Kernel.LogError(e.Message);
                onConnectionAoComplete(this, System.Net.Sockets.SocketAsyncOperation.Send, System.Net.Sockets.SocketError.OperationAborted);
            }
            catch (Exception e)
            {
                Kernel.LogError(e.Message);
                onConnectionAoComplete(this, System.Net.Sockets.SocketAsyncOperation.Send, System.Net.Sockets.SocketError.SocketError);
            }
        }
    }
}