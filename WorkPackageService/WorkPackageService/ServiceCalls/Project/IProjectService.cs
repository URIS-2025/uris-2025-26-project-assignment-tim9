namespace WorkPackageService.ServiceCalls.Project
{
    public interface IProjectService
    {
        Task<DateTime?> GetProjectDeadlineAsync(Guid projectId, string? authToken);
    }
}