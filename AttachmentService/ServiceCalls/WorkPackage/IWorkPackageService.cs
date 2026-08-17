using AttachmentService.Models.DTO.WorkPackage;

namespace AttachmentService.ServiceCalls.WorkPackage
{
    public interface IWorkPackageService
    {
        Task<WorkPackageDTO?> GetWorkPackageByIdAsync(Guid id);
    }
}
