using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// A minimal real HTTP server standing in for WorkPackage/Project Service in integration
    /// tests - a C# equivalent of the throwaway Python http.server scripts used to verify this
    /// manually during development. Uses HttpListener (built into .NET, no extra package)
    /// rather than a mock, so the real HttpClient/IHttpClientFactory/JSON-deserialization path
    /// in WorkPackageService/ProjectService actually gets exercised.
    /// </summary>
    public sealed class FakeJsonServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _loop;
        private readonly Func<string, (int StatusCode, string? Json)> _handler;

        public string BaseUrl { get; }

        public FakeJsonServer(Func<string, (int StatusCode, string? Json)> handler)
        {
            _handler = handler;

            var port = GetFreeTcpPort();
            BaseUrl = $"http://localhost:{port}/";

            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _listener.Start();

            _loop = Task.Run(ListenLoopAsync);
        }

        private async Task ListenLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch
                {
                    // Listener was stopped/disposed - exit the loop.
                    break;
                }

                var (statusCode, json) = _handler(context.Request.Url?.AbsolutePath ?? "/");
                context.Response.StatusCode = statusCode;

                if (json is not null)
                {
                    context.Response.ContentType = "application/json";
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await context.Response.OutputStream.WriteAsync(bytes);
                }

                context.Response.Close();
            }
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
            _listener.Close();
        }
    }
}
