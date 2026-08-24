using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Context;

namespace NotificationService.Tests.IntegrationTests
{
    // Jedna instanca po test klasi (preko IClassFixture<CustomWebApplicationFactory>) - svaka
    // dobija sopstvenu in-memory bazu (jedinstveno ime generisano ovde u konstruktoru), tako
    // da testovi iz razlicitih klasa ne dele stanje.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Ukloni pravu MySQL registraciju DbContext-a i zameni je in-memory verzijom.
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<NotificationServiceContext>));
                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                services.AddDbContext<NotificationServiceContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
            });
        }
    }
}
