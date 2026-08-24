using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PaymentService.Tests.Integration
{
    //pravi JWT tokene za testove, sa istim kljucem, izdavaocem i publikom kao
    //PaymentService/appsettings.json - dakle prolaze proveru kao pravi token iz AuthService-a
    internal static class TestTokens
    {
        private const string Key = "uris-2025-26-tim9-super-secret-key-min-32-chars";
        private const string Issuer = "AuthService";
        private const string Audience = "UrisApi";

        public static string ForRole(string role, Guid? userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
                new Claim(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}"),
                new Claim(ClaimTypes.Role, role)
            };

            return Build(claims);
        }

        //token bez identiteta korisnika - koristi se da se proveri odgovor 400
        public static string WithoutSubject(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}"),
                new Claim(ClaimTypes.Role, role)
            };

            return Build(claims);
        }

        private static string Build(IEnumerable<Claim> claims)
        {
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
