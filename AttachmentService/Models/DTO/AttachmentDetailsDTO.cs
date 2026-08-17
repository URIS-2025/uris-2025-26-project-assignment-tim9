namespace AttachmentService.Models.DTO
{
    public class AttachmentDetailsDTO
    {
        public AttachmentDTO Attachment { get; set; } = null!;

        public string? WorkPackageTitle { get; set; }

        public string? UploadedByUsername { get; set; }
        public string? UploadedByRole { get; set; }
    }
}
