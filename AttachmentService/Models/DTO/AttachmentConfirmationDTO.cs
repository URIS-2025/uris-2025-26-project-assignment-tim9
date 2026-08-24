using System.ComponentModel.DataAnnotations;

namespace AttachmentService.Models.DTO
{
    public class AttachmentConfirmationDTO
    {
        [Required]
        public Guid AttachmentId { get; set; }

        [RegularExpression("^[A-Fa-f0-9]{64}$", ErrorMessage = "Checksum must be a 64-character hexadecimal SHA-256 hash.")]
        public string? Checksum { get; set; }
    }
}
