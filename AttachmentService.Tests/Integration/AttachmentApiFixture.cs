using System.Net.Http.Headers;
using AttachmentService.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// Shared per-test-class setup: fake WorkPackage/Project/User services, the real app under
    /// test wired to a dedicated test database, an authenticated HttpClient for calling the app,
    /// and a plain HttpClient for calling MinIO directly (WebApplicationFactory's client routes
    /// everything through the in-process TestServer pipeline regardless of host, so it can't be
    /// used to reach a real external server like MinIO - a real HttpClient is needed for that
    /// half of the round trip, same as the separate curl calls used to verify this by hand
    /// during development).
    ///
    /// Because CheckMembershipAsync only receives a projectId (no userId) and returns the whole
    /// member list for the caller to filter client-side - exactly like the real ProjectService
    /// endpoint - the fake project server can't know in advance which user a given test will
    /// check. Its default response (for any project id other than <see cref="NonMemberProjectId"/>)
    /// is a fixed list containing exactly the five reserved user ids below; that is why every
    /// test that needs a working member/uploader uses one of them instead of a fresh Guid.
    /// </summary>
    public sealed class AttachmentApiFixture : IAsyncLifetime
    {
        // ---- Reserved ids the fake WorkPackage/Project/User servers know how to answer ----

        public Guid KnownTaskId { get; } = Guid.NewGuid();
        public const string KnownTaskTitle = "Integration Test Task";
        public Guid MissingTaskId { get; } = Guid.NewGuid();

        public Guid MissingProjectId { get; } = Guid.NewGuid();

        // Exists, but its member list is deliberately empty - used to prove non-admins are
        // rejected when they aren't an active member, and that Admins bypass that check.
        public Guid NonMemberProjectId { get; } = Guid.NewGuid();

        public Guid MemberUserId { get; } = Guid.NewGuid();
        public const string MemberUsername = "member.user";
        public const string MemberRole = "TeamMember";

        public Guid ProjectManagerUserId { get; } = Guid.NewGuid();
        public const string ProjectManagerUsername = "pm.user";
        public const string ProjectManagerRole = "ProjectManager";

        public Guid AdminUserId { get; } = Guid.NewGuid();
        public const string AdminUsername = "admin.user";
        public const string AdminRole = "Admin";

        public Guid ClientUserId { get; } = Guid.NewGuid();
        public const string ClientUsername = "client.user";
        public const string ClientRole = "Client";

        // A project member the fake User server does NOT recognize - used to prove uploader
        // identity enrichment degrades to null instead of failing the whole request.
        public Guid UnknownToUserServiceMemberId { get; } = Guid.NewGuid();

        private FakeJsonServer _taskServer = null!;
        private FakeJsonServer _projectServer = null!;
        private FakeJsonServer _userServer = null!;
        private AttachmentApiFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;
        public HttpClient StorageClient { get; } = new();

        public Task InitializeAsync()
        {
            _taskServer = new FakeJsonServer(path =>
            {
                var p = path.TrimStart('/');
                return p.StartsWith($"api/task/{KnownTaskId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"title\":\"{KnownTaskTitle}\"}}")
                    : (404, null);
            });

            _projectServer = new FakeJsonServer(path =>
            {
                var p = path.TrimStart('/');

                if (p.StartsWith("api/projectmember/project/", StringComparison.OrdinalIgnoreCase))
                {
                    var idPart = p["api/projectmember/project/".Length..];
                    if (string.Equals(idPart, NonMemberProjectId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return (200, "[]");
                    }

                    var members = string.Join(",", new[]
                    {
                        MemberUserId, ProjectManagerUserId, AdminUserId, ClientUserId, UnknownToUserServiceMemberId
                    }.Select(id => $"{{\"userId\":\"{id}\",\"status\":true}}"));
                    return (200, $"[{members}]");
                }

                if (p.StartsWith("api/project/", StringComparison.OrdinalIgnoreCase))
                {
                    var idPart = p["api/project/".Length..];
                    return string.Equals(idPart, MissingProjectId.ToString(), StringComparison.OrdinalIgnoreCase)
                        ? (404, null)
                        : (200, "{}");
                }

                return (404, null);
            });

            _userServer = new FakeJsonServer(path =>
            {
                var p = path.TrimStart('/');
                (Guid Id, string Username, string Role)[] known =
                {
                    (MemberUserId, MemberUsername, MemberRole),
                    (ProjectManagerUserId, ProjectManagerUsername, ProjectManagerRole),
                    (AdminUserId, AdminUsername, AdminRole),
                    (ClientUserId, ClientUsername, ClientRole)
                };

                foreach (var (id, username, role) in known)
                {
                    if (p.StartsWith($"api/user/{id}", StringComparison.OrdinalIgnoreCase))
                    {
                        return (200, $"{{\"username\":\"{username}\",\"email\":\"{username}@example.com\",\"role\":\"{role}\"}}");
                    }
                }

                return (404, null);
            });

            _factory = new AttachmentApiFactory(_taskServer.BaseUrl, _projectServer.BaseUrl, _userServer.BaseUrl);
            // Redirects (e.g. from /download) come back as-is instead of being followed, so
            // tests can assert on the 302 and its Location header directly.
            Client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.Generate());

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AttachmentContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AttachmentContext>();
                context.Database.EnsureDeleted();
            }

            Client.Dispose();
            StorageClient.Dispose();
            _factory.Dispose();
            _taskServer.Dispose();
            _projectServer.Dispose();
            _userServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
