using TimelogService.Models.DTO;

namespace TimelogService.Data
{
    public interface ITimelogRepository
    {
        IEnumerable<TimelogDTO> GetTimelogs(Guid? projectId = null, Guid? taskId = null);
        TimelogDTO? GetTimelogById(Guid id);
        Task<TimelogConfirmationDTO> CreateTimelogAsync(TimelogCreationDTO timelog, Guid loggedByUserId, string? bearerToken);
        Task<TimelogConfirmationDTO?> UpdateTimelogAsync(Guid id, TimelogUpdateDTO timelog, Guid actingUserId, string? bearerToken);
        Task DeleteTimelogAsync(Guid id, Guid actingUserId, string? bearerToken);
    }
}
