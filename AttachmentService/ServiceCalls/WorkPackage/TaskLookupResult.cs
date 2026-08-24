using AttachmentService.Models.DTO.WorkPackage;

namespace AttachmentService.ServiceCalls.WorkPackage
{
    public enum TaskLookupStatus
    {
        Found,
        NotFound,
        ServiceUnavailable
    }

    public record TaskLookupResult(TaskLookupStatus Status, TaskDTO? Task);
}
