namespace AttachmentService.Exceptions
{
    public class StorageUnavailableException : Exception
    {
        public StorageUnavailableException(Exception innerException)
            : base("The object storage service is unavailable.", innerException)
        {
        }
    }
}
