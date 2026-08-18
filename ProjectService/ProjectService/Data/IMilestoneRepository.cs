using ProjectService.Models;
using ProjectService.Models.DTO.MilestoneDtos;

namespace ProjectService.Data
{
    public interface IMilestoneRepository
    {
        IEnumerable<MilestoneDto> GetMilestones();
        IEnumerable<MilestoneDto> GetMilestonesByProjectId(Guid projectId);
        MilestoneDto GetMilestoneById(Guid milestoneId);
        MilestoneConfirmationDto CreateMilestone(MilestoneCreationDto milestone);
        MilestoneConfirmationDto? UpdateMilestone(MilestoneUpdateDto milestone);
        void DeleteMilestone(Guid milestoneId);
        bool SaveChanges();
    }
}
