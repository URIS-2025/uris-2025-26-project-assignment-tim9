using TimelogService.Models.DTO.WorkPackage;

namespace TimelogService.ServiceCalls.WorkPackage
{
    public interface IWorkPackageService
    {
        WorkPackageDTO GetWorkPackageById(Guid id);
    }
}
