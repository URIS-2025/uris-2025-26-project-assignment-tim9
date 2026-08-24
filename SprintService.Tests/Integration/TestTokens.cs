using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SprintService.Tests.Integration
{
    /// <summary>
    /// Mints JWTs for integration tests. Uses the same Jwt:Key/Issuer/Audience as
    /// SprintService/appsettings.json (Development doesn't override the Jwt section), so tokens
    /// minted here validate against the real app under test exactly like a real AuthService
    /// token would.
    /// </summary>
    internal static class TestTokens
    {
        private const string Key = "uris-2025-26-tim9-super-secret-key-min-32-chars";
        private const string Issuer = "AuthService";
        private const string Audience = "UrisApi";

        public static string ForRole(string role, Guid? userId = null)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
                new Claim(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}"),
                new Claim(ClaimTypes.Role, role)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
