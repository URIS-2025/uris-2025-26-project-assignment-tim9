using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectService.Context;

namespace ProjectService.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var optionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ProjectContext>));
                if (optionsDescriptor != null)
                    services.Remove(optionsDescriptor);

                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ProjectContext));
                if (contextDescriptor != null)
                    services.Remove(contextDescriptor);

                services.AddDbContext<ProjectContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
            });
        }
    }
}
