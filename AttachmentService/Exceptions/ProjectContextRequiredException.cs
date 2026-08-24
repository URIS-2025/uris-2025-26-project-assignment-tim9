namespace AttachmentService.Exceptions
{
    public class ProjectContextRequiredException : Exception
    {
        public Guid UserId { get; }

        public ProjectContextRequiredException(Guid userId)
            : base($"User '{userId}' must filter by projectId or taskId to list attachments (only Admins can list everything).")
        {
            UserId = userId;
        }
    }
}
