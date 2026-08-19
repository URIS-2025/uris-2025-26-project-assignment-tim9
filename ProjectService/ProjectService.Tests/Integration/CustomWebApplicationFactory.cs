using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProjectService.Context;
using ProjectService.ServiceCalls.User;

namespace ProjectService.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private const string JwtKey = "uris-2025-26-tim9-super-secret-key-min-32-chars";
        private const string JwtIssuer = "AuthService";
        private const string JwtAudience = "UrisApi";

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

                var userServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IUserService));
                if (userServiceDescriptor != null)
                    services.Remove(userServiceDescriptor);

                services.AddScoped<IUserService, FakeUserService>();
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
