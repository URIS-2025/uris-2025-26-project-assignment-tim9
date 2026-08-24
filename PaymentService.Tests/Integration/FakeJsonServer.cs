using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PaymentService.Tests.Integration
{
    //pravi HTTP server na slobodnom portu koji glumi User odnosno Project servis.
    //nije mock - ide preko stvarnog HttpClient-a, pa se testira i deserijalizacija odgovora
    public sealed class FakeJsonServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts = new();
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

            _ = Task.Run(ListenLoopAsync);
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
                    //listener je ugasen, izlazimo iz petlje
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
