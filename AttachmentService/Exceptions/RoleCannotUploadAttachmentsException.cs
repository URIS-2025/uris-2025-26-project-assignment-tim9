namespace AttachmentService.Exceptions
{
    public class RoleCannotUploadAttachmentsException : Exception
    {
        public Guid UserId { get; }
        public string Role { get; }

        public RoleCannotUploadAttachmentsException(Guid userId, string role)
            : base($"User '{userId}' with role '{role}' cannot upload attachments - only TeamMember, ProjectManager, or Admin can.")
        {
            UserId = userId;
            Role = role;
        }
    }
}
