using System.ComponentModel.DataAnnotations;

namespace AttachmentService.Models.DTO
{
    public class AttachmentUpdateDTO
    {
        [StringLength(255, MinimumLength = 1)]
        public string? FileName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
