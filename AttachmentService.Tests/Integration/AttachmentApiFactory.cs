using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// Boots the real app (real DI container, real routing, real AttachmentRepository,
    /// real S3 SDK) against a dedicated integration-test MySQL database (separate from the
    /// "attachment_db" used for manual dev, so this suite never collides with or depends on
    /// leftover dev data) and the same local MinIO already used throughout manual testing.
    /// WorkPackage/Project Service URLs point at FakeJsonServer instances supplied by the caller.
    /// </summary>
    public class AttachmentApiFactory : WebApplicationFactory<Program>
    {
        public const string TestDatabaseName = "attachment_db_integration_test";

        private readonly string _workPackageServiceUrl;
        private readonly string _projectServiceUrl;

        public AttachmentApiFactory(string workPackageServiceUrl, string projectServiceUrl)
        {
            _workPackageServiceUrl = workPackageServiceUrl;
            _projectServiceUrl = projectServiceUrl;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                // Appended after appsettings.*.json, so these win - the standard
                // WebApplicationFactory config-override pattern.
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:AttachmentDB"] = $"server=localhost;port=3306;database={TestDatabaseName};user=root;password=root",
                    ["ObjectStorage:ServiceUrl"] = "http://localhost:9000",
                    ["ObjectStorage:BucketName"] = "attachments",
                    ["ObjectStorage:AccessKey"] = "minioadmin",
                    ["ObjectStorage:SecretKey"] = "minioadmin",
                    ["ObjectStorage:ForcePathStyle"] = "true",
                    ["Services:WorkPackageService"] = _workPackageServiceUrl,
                    ["Services:ProjectService"] = _projectServiceUrl
                });
            });
        }
    }
}
