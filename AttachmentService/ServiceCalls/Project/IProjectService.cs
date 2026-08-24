namespace AttachmentService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<ProjectExistsResult> CheckProjectExistsAsync(Guid projectId, string? bearerToken);
        Task<ProjectMembershipResult> CheckMembershipAsync(Guid projectId, Guid userId, string? bearerToken);
    }
}
