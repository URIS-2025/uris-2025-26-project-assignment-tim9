using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SprintService.Tests.Integration
{
    /// <summary>
    /// Boots the real app against a dedicated integration-test MySQL database (separate from
    /// "SprintDB" used for manual dev) - Project Service URL points at a FakeJsonServer
    /// supplied by the caller.
    /// </summary>
    public class SprintApiFactory : WebApplicationFactory<Program>
    {
        public const string DefaultTestDatabaseName = "SprintDB_integration_test";

        private readonly string _projectServiceUrl;
        private readonly string _databaseName;

        public SprintApiFactory(string projectServiceUrl, string? databaseName = null)
        {
            _projectServiceUrl = projectServiceUrl;
            _databaseName = databaseName ?? DefaultTestDatabaseName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SprintDB"] = $"Server=localhost;Port=3306;Database={_databaseName};User=root;Password=root;",
                    ["Services:ProjectService"] = _projectServiceUrl
                });
            });
        }
    }
}
