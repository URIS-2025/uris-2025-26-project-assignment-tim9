using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SprintService.Context;

namespace SprintService.Tests.Integration
{
    /// <summary>
    /// Shared per-test-class setup: a fake Project Service and the real app under test wired
    /// to a dedicated test database, with an HttpClient for calling the app.
    /// </summary>
    public sealed class SprintApiFixture : IAsyncLifetime
    {
        public Guid KnownProjectId { get; } = Guid.NewGuid();
        public Guid KnownMilestoneId { get; } = Guid.NewGuid();
        public static readonly DateTime KnownExpectedDate = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        private FakeJsonServer _projectServer = null!;
        private SprintApiFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;

        public Task InitializeAsync()
        {
            _projectServer = new FakeJsonServer(path =>
                path.TrimStart('/').StartsWith($"api/project/{KnownProjectId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"milestoneID\":\"{KnownMilestoneId}\",\"expectedDate\":\"{KnownExpectedDate:O}\"}}")
                    : (404, null));

            _factory = new SprintApiFactory(_projectServer.BaseUrl);
            Client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SprintContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SprintContext>();
                context.Database.EnsureDeleted();
            }

            Client.Dispose();
            _factory.Dispose();
            _projectServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
