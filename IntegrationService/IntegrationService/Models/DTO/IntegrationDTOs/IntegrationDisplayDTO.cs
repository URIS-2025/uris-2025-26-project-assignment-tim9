namespace IntegrationService.Models.DTO.IntegrationDTOs
{
    public class IntegrationDisplayDTO
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        // Nikad ceo kljuc - samo maskirana verzija (npr. "****ab12").
        public string ApiKeyMasked { get; set; } = string.Empty;

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
