using TimelogService.Models.DTO;

namespace TimelogService.Data
{
    public interface ITimelogRepository
    {
        IEnumerable<TimelogDTO> GetTimelogs(Guid? projectId = null, Guid? workPackageId = null);
        TimelogDTO? GetTimelogById(Guid id);
        Task<TimelogConfirmationDTO> CreateTimelogAsync(TimelogCreationDTO timelog, Guid loggedByUserId);
        Task<TimelogConfirmationDTO?> UpdateTimelogAsync(Guid id, TimelogUpdateDTO timelog);
        void DeleteTimelog(Guid id);
    }
}
