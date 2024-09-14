using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Blanketmen.Hypnos
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

        public HttpConnection(int id, HandlerConfig handlerConfig)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetResponse(out IResponse response)
        {
            return pendingResponses.TryDequeue(out response);
        }

        public void SendAsync(IRequest request)
        {
            SendAsyncInternal(request as HttpRequest);
        }

        private async void SendAsyncInternal(HttpRequest request)
        {
            try
            {
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
                Logging.Error($"Send HttpRequest exception. Exception: {e.Message}", nameof(HttpConnection));
                onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.Fault);
            }
            catch (TaskCanceledException e)
            {
                Logging.Error($"Send HttpRequest exception. Exception: {e.Message}", nameof(HttpConnection));
                onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.OperationAborted);
            }
            catch (Exception e)
            {
                Logging.Error($"Send HttpRequest exception. Exception: {e.Message}", nameof(HttpConnection));
                onConnectionAoComplete(this, SocketAsyncOperation.Send, SocketError.SocketError);
            }
        }
    }
}