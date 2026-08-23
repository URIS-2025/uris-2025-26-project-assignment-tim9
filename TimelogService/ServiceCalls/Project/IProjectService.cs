namespace TimelogService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<ProjectMembershipResult> CheckMembershipAsync(Guid projectId, Guid userId, string? bearerToken);
        Task<ProjectExistsResult> CheckProjectExistsAsync(Guid projectId, string? bearerToken);
    }
}
