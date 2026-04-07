using TimelogService.Models;
using TimelogService.Models.DTO;

namespace TimelogService.Data
{
    public interface ITimelogRepository
    {
        IEnumerable<TimelogDTO> GetTimelogs();
        TimelogDTO GetTimelogById(Guid id);
        TimelogConfirmationDTO CreateTimelog(TimelogCreationDTO timelog);
        TimelogConfirmationDTO UpdateTimelog(Timelog timelog);
        void DeleteTimelog(Guid id);
        bool SaveChanges();
    }
}