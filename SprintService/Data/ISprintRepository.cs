using SprintService.Models.DTO;

namespace SprintService.Data
{
    public interface ISprintRepository
    {
        IEnumerable<SprintDTO> GetSprints(Guid? projectId = null);
        SprintDTO? GetSprintById(Guid id);
        Task<SprintConfirmationDTO> CreateSprintAsync(Guid projectId, SprintCreationDTO sprint);
        Task<SprintConfirmationDTO?> UpdateSprintAsync(Guid sprintId, SprintUpdateDTO sprint);
        void DeleteSprint(Guid id);
    }
}
