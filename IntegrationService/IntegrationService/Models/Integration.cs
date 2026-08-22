namespace IntegrationService.Models
{
    public class Integration
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        // Cuva se iskljucivo enkriptovano (preko IApiKeyProtector), nikad plain-text.
        public string ApiKeyEncrypted { get; set; } = string.Empty;

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
