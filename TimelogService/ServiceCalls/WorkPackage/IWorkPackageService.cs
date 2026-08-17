using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.ServiceCalls.WorkPackage
{
    public interface IWorkPackageService
    {
        Task<WorkPackageDTO?> GetWorkPackageByIdAsync(Guid id);
    }
}
