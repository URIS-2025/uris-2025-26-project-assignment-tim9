using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using AuthService.Services;
using Xunit;

namespace AuthService.Tests
{
    public class TokenServiceTests
    {
        private static ITokenService CreateService()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "test-secret-key-min-32-characters-long",
                    ["Jwt:Issuer"] = "AuthService",
                    ["Jwt:Audience"] = "UrisApi",
                    ["Jwt:AccessTokenExpirationMinutes"] = "15"
                })
                .Build();

            return new TokenService(configuration);
        }

        [Fact]
        public void GenerateAccessToken_ContainsExpectedClaims()
        {
            var service = CreateService();
            var userId = Guid.NewGuid();

            var (token, expiresAt) = service.GenerateAccessToken(userId, "admin", "Admin");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("AuthService", jwt.Issuer);
            Assert.Contains(jwt.Audiences, a => a == "UrisApi");
            Assert.Contains(jwt.Claims, c => c.Type == "sub" && c.Value == userId.ToString());
            Assert.Contains(jwt.Claims, c => c.Value == "Admin");
            Assert.True(expiresAt > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateRefreshToken_ProducesUniqueValues()
        {
            var service = CreateService();

            var token1 = service.GenerateRefreshToken();
            var token2 = service.GenerateRefreshToken();

            Assert.NotEqual(token1, token2);
            Assert.False(string.IsNullOrWhiteSpace(token1));
        }
    }
}
