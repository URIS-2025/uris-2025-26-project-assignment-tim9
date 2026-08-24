using Microsoft.Extensions.DependencyInjection;
using TimelogService.Context;

namespace TimelogService.Tests.Integration
{
    /// <summary>
    /// Shared per-test-class setup: fake Project/User/WorkPackage servers and the real app
    /// under test wired to a dedicated test database, with an HttpClient for calling the app.
    /// </summary>
    public sealed class TimelogApiFixture : IAsyncLifetime
    {
        public Guid KnownUserId { get; } = Guid.NewGuid();
        public const string KnownUsername = "integration.user";
        public const string KnownUserRole = "TeamMember";

        public Guid ClientUserId { get; } = Guid.NewGuid();

        public Guid AdminUserId { get; } = Guid.NewGuid();

        public Guid KnownTaskId { get; } = Guid.NewGuid();
        public const string KnownTaskTitle = "Integration Test Task";
        public const string KnownTaskStatus = "InProgress";

        public Guid MissingTaskId { get; } = Guid.NewGuid();

        public Guid NonMemberProjectId { get; } = Guid.NewGuid();

        public Guid MissingProjectId { get; } = Guid.NewGuid();

        private FakeJsonServer _projectServer = null!;
        private FakeJsonServer _userServer = null!;
        private FakeJsonServer _workPackageServer = null!;
        private TimelogApiFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;

        public Task InitializeAsync()
        {
            _projectServer = new FakeJsonServer(path =>
            {
                var trimmed = path.TrimStart('/');
                if (trimmed.StartsWith($"api/projectmember/project/{NonMemberProjectId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "[]");
                }
                if (trimmed.StartsWith("api/projectmember/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"[{{\"userId\":\"{KnownUserId}\",\"status\":true}},{{\"userId\":\"{ClientUserId}\",\"status\":true}}]");
                }
                if (trimmed.StartsWith($"api/project/{MissingProjectId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (404, null);
                }
                if (trimmed.StartsWith("api/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{\"name\":\"Fake Project\"}");
                }
                return (404, null);
            });

            _userServer = new FakeJsonServer(path =>
            {
                var trimmed = path.TrimStart('/');
                if (trimmed.StartsWith($"api/user/{KnownUserId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"{{\"username\":\"{KnownUsername}\",\"email\":\"integration.user@example.com\",\"role\":\"{KnownUserRole}\"}}");
                }
                if (trimmed.StartsWith($"api/user/{ClientUserId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{\"username\":\"client.user\",\"email\":\"client.user@example.com\",\"role\":\"Client\"}");
                }
                if (trimmed.StartsWith($"api/user/{AdminUserId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{\"username\":\"admin.user\",\"email\":\"admin.user@example.com\",\"role\":\"Admin\"}");
                }
                return (404, null);
            });

            _workPackageServer = new FakeJsonServer(path =>
            {
                var trimmed = path.TrimStart('/');
                if (trimmed.StartsWith($"api/task/{MissingTaskId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (404, null);
                }
                if (trimmed.StartsWith("api/task/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"{{\"title\":\"{KnownTaskTitle}\",\"status\":1}}");
                }
                return (404, null);
            });

            _factory = new TimelogApiFactory(_projectServer.BaseUrl, _userServer.BaseUrl, _workPackageServer.BaseUrl);
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
            _userServer.Dispose();
            _workPackageServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
