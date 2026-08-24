namespace AttachmentService.Exceptions
{
    public class ProjectNotFoundException : Exception
    {
        public Guid ProjectId { get; }

        public ProjectNotFoundException(Guid projectId)
            : base($"Project '{projectId}' does not exist.")
        {
            ProjectId = projectId;
        }
    }
}
