using AutoMapper;
using ProjectService.Context;
using ProjectService.Models;
using ProjectService.Models.DTO.RequirementsDtos;

namespace ProjectService.Data
{
    public class RequirementsRepository : IRequirementsRepository
    {
        private readonly ProjectContext _context;
        private readonly IMapper _mapper;

        public RequirementsRepository(ProjectContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<RequirementsDto> GetRequirements()
        {
            var requirements = _context.Requirements.ToList();
            var result = new List<RequirementsDto>();

            foreach (var requirement in requirements)
            {
                var dto = _mapper.Map<RequirementsDto>(requirement);
                result.Add(dto);
            }

            return result;
        }

        public IEnumerable<RequirementsDto> GetRequirementsByProjectId(Guid ProjectId)
        {
            var requirements = _context.Requirements
                .Where(r => r.ProjectId == ProjectId)
                .ToList();

            var result = new List<RequirementsDto>();
            foreach (var requirement in requirements)
            {
                var dto = _mapper.Map<RequirementsDto>(requirement);
                result.Add(dto);
            }

            return result;
        }

        public RequirementsDto GetRequirementById(Guid RequirementId)
        {
            var requirement = _context.Requirements
                .FirstOrDefault(r => r.RequirementId == RequirementId);

            if (requirement == null)
                return null;

            return _mapper.Map<RequirementsDto>(requirement);
        }

        public RequirementsConfirmationDto CreateRequirement(RequirementsCreationDto requirementDto)
        {
            var requirement = new Requirements
            {
                RequirementId = Guid.NewGuid(),
                ProjectId = requirementDto.ProjectId,
                Description = requirementDto.Description
            };

            _context.Requirements.Add(requirement);
            _context.SaveChanges();

            return new RequirementsConfirmationDto
            {
                RequirementId = requirement.RequirementId,
                ProjectId = requirement.ProjectId,
                Description = requirement.Description
            };
        }

        public RequirementsConfirmationDto UpdateRequirement(Requirements requirement)
        {
            var existing = _context.Requirements
                .FirstOrDefault(r => r.RequirementId == requirement.RequirementId);

            if (existing != null)
            {
                existing.ProjectId = requirement.ProjectId;
                existing.Description = requirement.Description;

                _context.SaveChanges();
            }

            return new RequirementsConfirmationDto
            {
                RequirementId = requirement.RequirementId,
                ProjectId = requirement.ProjectId,
                Description = requirement.Description
            };
        }

        public void DeleteRequirement(Guid RequirementId)
        {
            var requirement = _context.Requirements
                .FirstOrDefault(r => r.RequirementId == RequirementId);

            if (requirement != null)
            {
                _context.Remove(requirement);
                _context.SaveChanges();
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
