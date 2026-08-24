namespace AttachmentService.Exceptions
{
    public class NotAttachmentOwnerException : Exception
    {
        public Guid ActingUserId { get; }
        public Guid OwnerUserId { get; }

        public NotAttachmentOwnerException(Guid actingUserId, Guid ownerUserId)
            : base($"User '{actingUserId}' can only update or delete attachments they uploaded themselves.")
        {
            ActingUserId = actingUserId;
            OwnerUserId = ownerUserId;
        }
    }
}
