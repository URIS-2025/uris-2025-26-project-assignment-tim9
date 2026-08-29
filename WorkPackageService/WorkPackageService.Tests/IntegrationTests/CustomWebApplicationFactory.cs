using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using WorkPackageService.Context;
using WorkPackageService.ServiceCalls.Notification;

namespace WorkPackageService.Tests.IntegrationTests
{
    // Jedna instanca po test klasi (preko IClassFixture<CustomWebApplicationFactory>) - svaka
    // dobija sopstvenu in-memory bazu (jedinstveno ime generisano ovde u konstruktoru), tako
    // da testovi iz razlicitih klasa ne dele stanje.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private const string JwtKey = "uris-2025-26-tim9-super-secret-key-min-32-chars";
        private const string JwtIssuer = "AuthService";
        private const string JwtAudience = "UrisApi";

        private readonly string _databaseName = Guid.NewGuid().ToString();

        public Mock<INotificationService> NotificationServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Ukloni pravu MySQL registraciju DbContext-a i zameni je in-memory verzijom.
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<WorkPackageServiceContext>));
                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                services.AddDbContext<WorkPackageServiceContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Zameni pravi HTTP-bazirani INotificationService mokovanom verzijom - testovi ne
                // smeju da pokusavaju prave pozive ka nepostojecem Notification servisu.
                var notificationServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(INotificationService));
                if (notificationServiceDescriptor != null)
                {
                    services.Remove(notificationServiceDescriptor);
                }

                NotificationServiceMock
                    .Setup(s => s.SendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                services.AddSingleton(NotificationServiceMock.Object);
            });
        }

        public string GenerateJwtToken(Guid? userId = null, string? role = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
                new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
