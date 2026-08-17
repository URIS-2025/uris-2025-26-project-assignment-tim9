using Microsoft.Extensions.DependencyInjection;
using TimelogService.Context;

namespace TimelogService.Tests.Integration
{
    /// <summary>
    /// Shared per-test-class setup: fake Project/WorkPackage servers and the real app under
    /// test wired to a dedicated test database, with an HttpClient for calling the app.
    /// </summary>
    public sealed class TimelogApiFixture : IAsyncLifetime
    {
        public Guid KnownUserId { get; } = Guid.NewGuid();
        public const string KnownUsername = "integration.user";

        public Guid KnownWorkPackageId { get; } = Guid.NewGuid();
        public const string KnownWorkPackageTitle = "Integration Test Task";

        private FakeJsonServer _projectServer = null!;
        private FakeJsonServer _workPackageServer = null!;
        private TimelogApiFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;

        public Task InitializeAsync()
        {
            _projectServer = new FakeJsonServer(path =>
                path.TrimStart('/').StartsWith($"api/project/users/{KnownUserId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"username\":\"{KnownUsername}\",\"email\":\"integration.user@example.com\"}}")
                    : (404, null));

            _workPackageServer = new FakeJsonServer(path =>
                path.TrimStart('/').StartsWith($"api/workpackage/{KnownWorkPackageId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"title\":\"{KnownWorkPackageTitle}\",\"status\":\"InProgress\"}}")
                    : (404, null));

            _factory = new TimelogApiFactory(_projectServer.BaseUrl, _workPackageServer.BaseUrl);
            Client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TimelogContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TimelogContext>();
                context.Database.EnsureDeleted();
            }

            Client.Dispose();
            _factory.Dispose();
            _projectServer.Dispose();
            _workPackageServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
