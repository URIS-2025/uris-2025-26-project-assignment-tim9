using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Context;

namespace PaymentService.Tests.Integration
{
    //zajednicka priprema za ceo test razred: lazni User i Project servis, podignuta
    //aplikacija i prazna test baza sa pocetnim podacima
    public sealed class PaymentApiFixture : IAsyncLifetime
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public Guid KnownUserId { get; } = Guid.NewGuid();
        public const string KnownUsername = "integracioni.korisnik";

        public Guid KnownProjectId { get; } = Guid.NewGuid();
        public const string KnownProjectName = "Integracioni projekat";

        private FakeJsonServer _userServer = null!;
        private FakeJsonServer _projectServer = null!;
        private PaymentApiFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;

        public Task InitializeAsync()
        {
            _userServer = new FakeJsonServer(path =>
                path.TrimStart('/').Equals($"api/user/{KnownUserId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"name\":\"Integracioni Korisnik\",\"username\":\"{KnownUsername}\",\"email\":\"test@example.com\"}}")
                    : (404, null));

            _projectServer = new FakeJsonServer(path =>
                path.TrimStart('/').Equals($"api/project/{KnownProjectId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"projectId\":\"{KnownProjectId}\",\"name\":\"{KnownProjectName}\",\"budget\":50000}}")
                    : (404, null));

            _factory = new PaymentApiFactory(_userServer.BaseUrl, _projectServer.BaseUrl);
            Client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
                context.Database.EnsureDeleted();
            }

            Client.Dispose();
            _factory.Dispose();
            _userServer.Dispose();
            _projectServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
