namespace AttachmentService.Storage
{
 
    public class ObjectStorageOptions
    {
        
        public string ServiceUrl { get; set; } = string.Empty;

        public string BucketName { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public bool ForcePathStyle { get; set; } = true;
    }
}
