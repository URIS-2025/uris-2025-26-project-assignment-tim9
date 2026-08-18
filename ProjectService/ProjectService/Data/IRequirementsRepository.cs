using ProjectService.Models;
using ProjectService.Models.DTO.RequirementsDtos;

namespace ProjectService.Data
{
    public interface IRequirementsRepository
    {
        IEnumerable<RequirementsDto> GetRequirements();
        IEnumerable<RequirementsDto> GetRequirementsByProjectId(Guid ProjectId);
        RequirementsDto GetRequirementById(Guid RequirementId);
        RequirementsConfirmationDto CreateRequirement(RequirementsCreationDto requirement);
        RequirementsConfirmationDto UpdateRequirement(RequirementsUpdateDto requirement);
        void DeleteRequirement(Guid RequirementId);
        bool SaveChanges();
    }
}
