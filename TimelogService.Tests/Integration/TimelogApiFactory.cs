using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TimelogService.Tests.Integration
{
    /// <summary>
    /// Boots the real app against a dedicated integration-test MySQL database (separate from
    /// "TimelogDB" used for manual dev) - Project Service/WorkPackage Service URLs point at
    /// FakeJsonServer instances supplied by the caller.
    /// </summary>
    public class TimelogApiFactory : WebApplicationFactory<Program>
    {
        public const string DefaultTestDatabaseName = "TimelogDB_integration_test";

        private readonly string _projectServiceUrl;
        private readonly string _workPackageServiceUrl;
        private readonly string _databaseName;

        public TimelogApiFactory(string projectServiceUrl, string workPackageServiceUrl, string? databaseName = null)
        {
            _projectServiceUrl = projectServiceUrl;
            _workPackageServiceUrl = workPackageServiceUrl;
            _databaseName = databaseName ?? DefaultTestDatabaseName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TimelogDB"] = $"Server=localhost;Port=3306;Database={_databaseName};User=root;Password=root;",
                    ["Services:ProjectService"] = _projectServiceUrl,
                    ["Services:WorkPackageService"] = _workPackageServiceUrl
                });
            });
        }
    }
}
