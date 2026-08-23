using WorkPackageService.Models.DTO.WorkPackageDTOs;

namespace WorkPackageService.Data
{
    public interface IWorkPackageRepository
    {
        IEnumerable<WorkPackageDisplayDTO> GetAll();
        IEnumerable<WorkPackageDisplayDTO> GetByProjectId(Guid projectId);
        WorkPackageDisplayDTO? GetById(Guid id);
        WorkPackageDisplayDTO Add(WorkPackageCreateDTO dto);
        WorkPackageDisplayDTO? Update(Guid id, WorkPackageUpdateDTO dto);
        bool Delete(Guid id);
        bool SaveChanges();
    }
}
