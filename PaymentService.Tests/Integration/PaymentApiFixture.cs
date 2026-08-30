using System.Net.Http.Headers;
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

        //projekat iz seed podataka PaymentContext-a (project1 - "Project Management System")
        public static readonly Guid SeededProjectId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        public const string KnownProjectName = "Integracioni projekat";

        private FakeJsonServer _userServer = null!;
        private FakeJsonServer _projectServer = null!;
        private PaymentApiFactory _factory = null!;

        //podrazumevani klijent je prijavljen kao ProjectManager, jer vecina operacija to trazi
        public HttpClient Client { get; private set; } = null!;

        //klijent bez tokena, za provere odgovora 401
        public HttpClient AnonymousClient { get; private set; } = null!;

        public Task InitializeAsync()
        {
            _userServer = new FakeJsonServer(path =>
                path.TrimStart('/').Equals($"api/user/{KnownUserId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"name\":\"Integracioni Korisnik\",\"username\":\"{KnownUsername}\",\"email\":\"test@example.com\"}}")
                    : (404, null));

            _projectServer = new FakeJsonServer(path =>
            {
                var p = path.TrimStart('/');

                if (p.Equals($"api/project/{KnownProjectId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"{{\"projectId\":\"{KnownProjectId}\",\"name\":\"{KnownProjectName}\",\"budget\":50000}}");
                }

                //clanovi projekta - samo KnownUserId je aktivan clan
                if (p.Equals($"api/projectmember/project/{KnownProjectId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"[{{\"userId\":\"{KnownUserId}\",\"status\":true}}]");
                }

                //projekti na kojima je KnownUserId clan: test projekat i onaj iz seed podataka
                if (p.Equals($"api/project/user/{KnownUserId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200,
                        $"[{{\"projectId\":\"{KnownProjectId}\",\"name\":\"{KnownProjectName}\",\"budget\":50000}},"
                        + $"{{\"projectId\":\"{SeededProjectId}\",\"name\":\"Seeded\",\"budget\":10000}}]");
                }

                return (404, null);
            });

            _factory = new PaymentApiFactory(_userServer.BaseUrl, _projectServer.BaseUrl);

            Client = CreateClientFor("ProjectManager", KnownUserId);
            AnonymousClient = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return Task.CompletedTask;
        }

        //klijent prijavljen u zadatoj ulozi
        public HttpClient CreateClientFor(string role, Guid? userId = null)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokens.ForRole(role, userId));
            return client;
        }

        //klijent sa ispravnim tokenom, ali bez identiteta korisnika u njemu
        public HttpClient CreateClientWithoutSubject(string role)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokens.WithoutSubject(role));
            return client;
        }

        public Task DisposeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
                context.Database.EnsureDeleted();
            }

            Client.Dispose();
            AnonymousClient.Dispose();
            _factory.Dispose();
            _userServer.Dispose();
            _projectServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
