namespace AttachmentService.Models.DTO
{
    public class AttachmentUploadResponseDTO
    {
        public AttachmentDTO Attachment { get; set; } = null!;
        public string UploadUrl { get; set; } = string.Empty;
        public string UploadedByUsername { get; set; } = string.Empty;
        public string UploadedByRole { get; set; } = string.Empty;
    }
}
