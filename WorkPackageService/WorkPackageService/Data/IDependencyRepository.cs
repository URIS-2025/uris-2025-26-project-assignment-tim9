using WorkPackageService.Models.DTO.DependencyDTOs;

namespace WorkPackageService.Data
{
    public interface IDependencyRepository
    {
        IEnumerable<DependencyDisplayDTO> GetAll();
        DependencyDisplayDTO? GetById(Guid id);
        DependencyDisplayDTO? Add(DependencyCreateDTO dto);
        DependencyDisplayDTO? Update(Guid id, DependencyUpdateDTO dto);
        bool Delete(Guid id);
        bool SaveChanges();
    }
}
