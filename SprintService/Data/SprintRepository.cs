using AutoMapper;
using SprintService.Context;
using SprintService.Models;
using SprintService.Models.DTO;
using SprintService.ServiceCalls.Project;

namespace SprintService.Data
{
    public class SprintRepository : ISprintRepository
    {
        private readonly SprintContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public SprintRepository(SprintContext context, IMapper mapper, IProjectService projectService)
        {
            _context = context;
            _mapper = mapper;
            _projectService = projectService;
        }

        public IEnumerable<SprintDTO> GetSprints(Guid? projectId = null)
        {
            var query = _context.Sprints.AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(s => s.ProjectId == projectId.Value);
            }

            var sprints = query.ToList();
            return _mapper.Map<List<SprintDTO>>(sprints);
        }

        public SprintDTO? GetSprintById(Guid id)
        {
            var sprint = _context.Sprints.FirstOrDefault(s => s.Id == id);
            return sprint is null ? null : _mapper.Map<SprintDTO>(sprint);
        }

        public async Task<SprintConfirmationDTO> CreateSprintAsync(Guid projectId, SprintCreationDTO sprint)
        {
            var newSprint = _mapper.Map<Sprint>(sprint);
            newSprint.Id = Guid.NewGuid();
            newSprint.ProjectId = projectId;

            _context.Sprints.Add(newSprint);
            _context.SaveChanges();

            var confirmation = _mapper.Map<SprintConfirmationDTO>(newSprint);

            var projectData = await _projectService.GetProjectByIdAsync(newSprint.ProjectId);
            if (projectData is not null)
            {
                confirmation.MilestoneId = projectData.MilestoneID;
                confirmation.ExpectedDate = projectData.ExpectedDate;
            }

            return confirmation;
        }

        public async Task<SprintConfirmationDTO?> UpdateSprintAsync(Guid sprintId, SprintUpdateDTO sprint)
        {
            var existingSprint = _context.Sprints.FirstOrDefault(s => s.Id == sprintId);
            if (existingSprint is null)
            {
                return null;
            }

            _mapper.Map(sprint, existingSprint);
            _context.SaveChanges();

            var confirmation = _mapper.Map<SprintConfirmationDTO>(existingSprint);

            var projectData = await _projectService.GetProjectByIdAsync(existingSprint.ProjectId);
            if (projectData is not null)
            {
                confirmation.MilestoneId = projectData.MilestoneID;
                confirmation.ExpectedDate = projectData.ExpectedDate;
            }

            return confirmation;
        }

        public void DeleteSprint(Guid id)
        {
            var sprint = _context.Sprints.FirstOrDefault(s => s.Id == id);
            if (sprint is not null)
            {
                _context.Sprints.Remove(sprint);
                _context.SaveChanges();
            }
        }
    }
}
