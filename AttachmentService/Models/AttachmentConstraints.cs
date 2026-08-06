namespace AttachmentService.Models
{
    
    public static class AttachmentConstraints
    {
        public const long MaxFileSizeBytes = 25L * 1024 * 1024;

        public static readonly string[] AllowedContentTypes =
        {
            "image/png",
            "image/jpeg",
            "image/gif",
            "image/webp",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain",
            "text/csv",
            "application/zip",
        };
    }
}
