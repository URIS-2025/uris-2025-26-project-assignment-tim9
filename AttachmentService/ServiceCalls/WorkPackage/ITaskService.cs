namespace AttachmentService.ServiceCalls.WorkPackage
{
    public interface ITaskService
    {
        Task<TaskLookupResult> GetTaskByIdAsync(Guid id, string? bearerToken);
    }
}
