namespace AttachmentService.Storage
{
    public interface IFileStorageService
    {
        //a short-lived, signed URL the client PUTs the file's bytes to directly
        string GenerateUploadUrl(string storagePath, string contentType, TimeSpan? expiry = null);

        //a short-lived, signed URL to read the file back
        string GenerateDownloadUrl(string storagePath, TimeSpan? expiry = null);

        //calls out to storage
        Task<bool> ObjectExistsAsync(string storagePath);
    }
}
