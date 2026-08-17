using AttachmentService.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// Shared per-test-class setup: two fake external services, the real app under test wired
    /// to a dedicated test database, an HttpClient for calling the app, and a plain HttpClient
    /// for calling MinIO directly (WebApplicationFactory's client routes everything through the
    /// in-process TestServer pipeline regardless of host, so it can't be used to reach a real
    /// external server like MinIO - a real HttpClient is needed for that half of the round trip,
    /// same as the separate curl calls used to verify this by hand during development).
    /// </summary>
    public sealed class AttachmentApiFixture : IAsyncLifetime
    {
        public Guid KnownWorkPackageId { get; } = Guid.NewGuid();
        public const string KnownWorkPackageTitle = "Integration Test Task";

        public Guid KnownUploaderId { get; } = Guid.NewGuid();
        public const string KnownUploaderUsername = "integration.user";
        public const string KnownUploaderRole = "Tester";

        private FakeJsonServer _workPackageServer = null!;
        private FakeJsonServer _projectServer = null!;
        private AttachmentApiFactory _factory = null!;

        public HttpClient Client { get; private set; } = null!;
        public HttpClient StorageClient { get; } = new();

        public Task InitializeAsync()
        {
            _workPackageServer = new FakeJsonServer(path =>
                path.TrimStart('/').StartsWith($"api/workpackage/{KnownWorkPackageId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"title\":\"{KnownWorkPackageTitle}\"}}")
                    : (404, null));

            _projectServer = new FakeJsonServer(path =>
                path.TrimStart('/').StartsWith($"api/project/users/{KnownUploaderId}", StringComparison.OrdinalIgnoreCase)
                    ? (200, $"{{\"username\":\"{KnownUploaderUsername}\",\"role\":\"{KnownUploaderRole}\"}}")
                    : (404, null));

            _factory = new AttachmentApiFactory(_workPackageServer.BaseUrl, _projectServer.BaseUrl);
            // Redirects (e.g. from /download) come back as-is instead of being followed, so
            // tests can assert on the 302 and its Location header directly.
            Client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

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
            _workPackageServer.Dispose();
            _projectServer.Dispose();

            return Task.CompletedTask;
        }
    }
}
