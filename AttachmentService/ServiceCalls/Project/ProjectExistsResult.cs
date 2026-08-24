namespace AttachmentService.ServiceCalls.Project
{
    public enum ProjectExistsStatus
    {
        Exists,
        NotFound,
        ServiceUnavailable
    }

    public record ProjectExistsResult(ProjectExistsStatus Status);
}
