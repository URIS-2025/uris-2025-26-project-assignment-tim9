namespace IntegrationService.Security
{
    public interface IApiKeyProtector
    {
        string Protect(string plainApiKey);

        string Unprotect(string encryptedApiKey);

        // Za prikaz korisniku - nikad ne vraca ceo kljuc, samo poslednja 4 karaktera.
        string Mask(string plainApiKey);
    }
}
