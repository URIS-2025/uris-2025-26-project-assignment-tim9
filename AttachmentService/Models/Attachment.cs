using AttachmentService.Models.Enums;

namespace AttachmentService.Models
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        public string? Checksum { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Description { get; set; }

        public AttachmentStatus Status { get; set; } = AttachmentStatus.Uploading;
        public DateTime? DeletedAt { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? TaskId { get; set; }

        public Guid UploadedByUserId { get; set; }
    }
}
