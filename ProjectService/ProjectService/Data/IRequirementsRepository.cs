using ProjectService.Models;
using ProjectService.Models.DTO.RequirementsDtos;

namespace ProjectService.Data
{
    public interface IRequirementsRepository
    {
        IEnumerable<RequirementsDto> GetRequirements();
        IEnumerable<RequirementsDto> GetRequirementsByProjectId(Guid projectId);
        RequirementsDto GetRequirementById(Guid requirementId);
        RequirementsConfirmationDto CreateRequirement(RequirementsCreationDto requirement);
        RequirementsConfirmationDto? UpdateRequirement(RequirementsUpdateDto requirement);
        void DeleteRequirement(Guid requirementId);
        bool SaveChanges();
    }
}
