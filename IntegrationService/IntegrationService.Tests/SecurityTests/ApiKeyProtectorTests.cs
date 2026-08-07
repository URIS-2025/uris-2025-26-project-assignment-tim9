using IntegrationService.Security;
using Microsoft.AspNetCore.DataProtection;

namespace IntegrationService.Tests.SecurityTests
{
    public class ApiKeyProtectorTests
    {
        private static IApiKeyProtector CreateProtector()
        {
            var provider = DataProtectionProvider.Create("IntegrationService.Tests");
            return new DataProtectionApiKeyProtector(provider);
        }

        [Fact]
        public void Protect_ThenUnprotect_ReturnsOriginalValue()
        {
            var protector = CreateProtector();
            var plainKey = "ghp_abcdefgh12345678";

            var encrypted = protector.Protect(plainKey);
            var decrypted = protector.Unprotect(encrypted);

            Assert.Equal(plainKey, decrypted);
        }

        [Fact]
        public void Protect_DoesNotReturnPlainTextValue()
        {
            var protector = CreateProtector();
            var plainKey = "ghp_abcdefgh12345678";

            var encrypted = protector.Protect(plainKey);

            Assert.DoesNotContain(plainKey, encrypted);
        }

        [Theory]
        [InlineData("ghp_abcdefgh12345678", "****************5678")]
        [InlineData("abcd", "abcd")]
        [InlineData("ab", "ab")]
        public void Mask_KeepsOnlyLastFourCharactersVisible(string plainKey, string expected)
        {
            var protector = CreateProtector();

            var masked = protector.Mask(plainKey);

            Assert.Equal(expected, masked);
        }
    }
}
