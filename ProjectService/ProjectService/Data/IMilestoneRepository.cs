using ProjectService.Models;
using ProjectService.Models.DTO.MilestoneDtos;

namespace ProjectService.Data
{
    public interface IMilestoneRepository
    {
        IEnumerable<MilestoneDto> GetMilestones();
        IEnumerable<MilestoneDto> GetMilestonesByProjectId(Guid ProjectId);
        MilestoneDto GetMilestoneById(Guid MilestoneId);
        MilestoneConfirmationDto CreateMilestone(MilestoneCreationDto milestone);
        MilestoneConfirmationDto UpdateMilestone(Milestone milestone);
        void DeleteMilestone(Guid MilestoneId);
        bool SaveChanges();
    }
}
