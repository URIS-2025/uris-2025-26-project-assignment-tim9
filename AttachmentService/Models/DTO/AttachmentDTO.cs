using AttachmentService.Models.Enums;

namespace AttachmentService.Models.DTO
{
    public class AttachmentDTO
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Checksum { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
        public AttachmentStatus Status { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? WorkPackageId { get; set; }
        public Guid UploadedByUserId { get; set; }
    }
}
