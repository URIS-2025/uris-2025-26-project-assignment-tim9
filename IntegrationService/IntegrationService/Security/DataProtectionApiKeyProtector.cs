using Microsoft.AspNetCore.DataProtection;

namespace IntegrationService.Security
{
    // Koristi ASP.NET Core Data Protection API (ugradjen mehanizam za enkripciju u okviru
    // frameworka) umesto rucne kriptografije, sa dedikovanim "purpose" stringom da se
    // izolije od ostalih upotreba Data Protection-a u istoj aplikaciji.
    public class DataProtectionApiKeyProtector : IApiKeyProtector
    {
        private readonly IDataProtector _protector;

        public DataProtectionApiKeyProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("IntegrationService.ApiKey.v1");
        }

        public string Protect(string plainApiKey) => _protector.Protect(plainApiKey);

        public string Unprotect(string encryptedApiKey) => _protector.Unprotect(encryptedApiKey);

        public string Mask(string plainApiKey)
        {
            if (string.IsNullOrEmpty(plainApiKey))
            {
                return string.Empty;
            }

            var visible = plainApiKey.Length <= 4 ? plainApiKey.Length : 4;
            return new string('*', Math.Max(plainApiKey.Length - visible, 0)) + plainApiKey[^visible..];
        }
    }
}
