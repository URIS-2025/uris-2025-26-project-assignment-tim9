using WorkPackageService.Models.DTO.BacklogDTOs;

namespace WorkPackageService.Data
{
    public interface IBacklogRepository
    {
        IEnumerable<BacklogDisplayDTO> GetAll();
        BacklogDisplayDTO? GetById(Guid id);
        BacklogDisplayDTO Add(BacklogCreateDTO dto);
        BacklogDisplayDTO? Update(Guid id, BacklogUpdateDTO dto);
        bool Delete(Guid id);
        bool SaveChanges();
    }
}
