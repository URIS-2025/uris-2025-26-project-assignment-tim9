using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AttachmentService.Tests.Integration
{
    /// <summary>
    /// Mints a JWT signed with the same shared dev key/issuer/audience Program.cs validates
    /// against, so integration tests can satisfy the controller's [Authorize] attribute exactly
    /// like a real caller forwarded through the API Gateway would. The acting user's identity
    /// for authorization purposes still comes from the X-User-Id header, not from any claim in
    /// this token - it only needs to be a validly signed, unexpired token.
    /// </summary>
    internal static class TestJwt
    {
        private const string Key = "uris-2025-26-tim9-super-secret-key-min-32-chars";
        private const string Issuer = "AuthService";
        private const string Audience = "UrisApi";

        public static string Generate()
        {
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: new[] { new Claim(ClaimTypes.Name, "integration-test-caller") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
