using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.ServiceCalls.WorkPackage
{
    public enum TaskLookupStatus
    {
        Found,
        NotFound,
        ServiceUnavailable
    }

    public record TaskLookupResult(TaskLookupStatus Status, TaskDTO? Task);
}
