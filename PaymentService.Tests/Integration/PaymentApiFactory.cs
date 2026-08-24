using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PaymentService.Tests.Integration
{
    //dize celu aplikaciju u memoriji, ali sa pravom MySQL bazom odvojenom od razvojne,
    //a adrese User i Project servisa usmerava na lazne servere
    public class PaymentApiFactory : WebApplicationFactory<Program>
    {
        public const string TestDatabaseName = "PaymentDB_integration_test";

        private readonly string _userServiceUrl;
        private readonly string _projectServiceUrl;

        public PaymentApiFactory(string userServiceUrl, string projectServiceUrl)
        {
            _userServiceUrl = userServiceUrl;
            _projectServiceUrl = projectServiceUrl;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PaymentDB"] = BuildConnectionString(),
                    ["Services:UserService"] = _userServiceUrl,
                    ["Services:ProjectService"] = _projectServiceUrl
                });
            });
        }

        //lozinka se cita iz iste environment varijable koju koristi i aplikacija,
        //samo se ime baze zameni imenom test baze. tako lozinka ne stoji u kodu.
        private static string BuildConnectionString()
        {
            var configured = Environment.GetEnvironmentVariable("ConnectionStrings__PaymentDB");

            if (!string.IsNullOrWhiteSpace(configured))
            {
                return Regex.Replace(configured, "Database=[^;]*", $"Database={TestDatabaseName}");
            }

            return $"Server=localhost;Port=3306;Database={TestDatabaseName};User=root;Password=root;";
        }
    }
}
