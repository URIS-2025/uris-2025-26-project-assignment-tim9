using System.Net.Http.Headers;
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
            // Mirrors ProjectService's real contract: GET api/milestone/project/{projectId}
            // returns a JSON array of milestones (ProjectService.Models.DTO.MilestoneDtos.MilestoneDto),
            // not a single object at api/project/{id}. Separately, GET api/project/{id} is the
            // existence check SprintRepository now runs before every create/update - every
            // project ID used elsewhere in this fixture's tests is treated as real here, since
            // existence-check *rejection* itself is covered by dedicated tests with their own
            // fixture instance below (e.g. CreateSprint_WithNonexistentProject_ReturnsBadRequest).
            _projectServer = new FakeJsonServer(request =>
            {
                var path = request.Url?.AbsolutePath.TrimStart('/') ?? "";

                if (path.StartsWith($"api/milestone/project/{KnownProjectId}", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, $"[{{\"milestoneId\":\"{KnownMilestoneId}\",\"projectId\":\"{KnownProjectId}\",\"expectedDate\":\"{KnownExpectedDate:O}\"}}]");
                }

                if (path.StartsWith("api/milestone/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (204, null); // some other project - exists, just no milestones
                }

                if (path.StartsWith("api/project/user/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "[]"); // only exercised by dedicated Client-role tests below
                }

                if (path.StartsWith("api/project/", StringComparison.OrdinalIgnoreCase))
                {
                    return (200, "{}");
                }

                return (404, null);
            });

            _factory = new SprintApiFactory(_projectServer.BaseUrl);
            Client = _factory.CreateClient();
            // SprintService itself now requires a valid JWT on every endpoint. Admin by default
            // so every existing CRUD/validation test keeps behaving exactly as before (full
            // access, no project scoping) - role-specific behavior gets its own dedicated tests
            // with their own client/token below, per-request headers override this default.
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.ForRole("Admin"));

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
