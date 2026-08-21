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

        public async Task<IEnumerable<SprintDTO>> GetSprintsForCallerAsync(Guid? projectId, Guid? clientUserId)
        {
            if (projectId is not null)
            {
                await EnsureProjectExistsAsync(projectId.Value);
            }

            var sprints = GetSprints(projectId);
            if (clientUserId is null)
            {
                return sprints;
            }

            var allowedProjectIds = await _projectService.GetProjectIdsForUserAsync(clientUserId.Value);
            var allowed = allowedProjectIds.ToHashSet();
            return sprints.Where(s => allowed.Contains(s.ProjectId));
        }

        public async Task<SprintDTO?> GetSprintByIdForCallerAsync(Guid sprintId, Guid? clientUserId)
        {
            var sprint = GetSprintById(sprintId);
            if (sprint is null || clientUserId is null)
            {
                return sprint;
            }

            var allowedProjectIds = await _projectService.GetProjectIdsForUserAsync(clientUserId.Value);
            return allowedProjectIds.Contains(sprint.ProjectId) ? sprint : null;
        }

        public async Task<SprintConfirmationDTO> CreateSprintAsync(Guid projectId, SprintCreationDTO sprint)
        {
            await EnsureProjectExistsAsync(projectId);

            var projectData = await _projectService.GetProjectByIdAsync(projectId);
            EnsureEndDateDoesNotPassMilestone(sprint.EndDate, projectData);

            var newSprint = _mapper.Map<Sprint>(sprint);
            newSprint.Id = Guid.NewGuid();
            newSprint.ProjectId = projectId;

            _context.Sprints.Add(newSprint);
            _context.SaveChanges();

            var confirmation = _mapper.Map<SprintConfirmationDTO>(newSprint);
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

            await EnsureProjectExistsAsync(sprint.ProjectId);

            var projectData = await _projectService.GetProjectByIdAsync(sprint.ProjectId);
            EnsureEndDateDoesNotPassMilestone(sprint.EndDate, projectData);

            _mapper.Map(sprint, existingSprint);
            _context.SaveChanges();

            var confirmation = _mapper.Map<SprintConfirmationDTO>(existingSprint);
            if (projectData is not null)
            {
                confirmation.MilestoneId = projectData.MilestoneID;
                confirmation.ExpectedDate = projectData.ExpectedDate;
            }

            return confirmation;
        }

        private static void EnsureEndDateDoesNotPassMilestone(
            DateTime sprintEndDate, Models.DTO.Project.MilestoneDTO? projectData)
        {
            if (projectData is not null && sprintEndDate > projectData.ExpectedDate)
            {
                throw new SprintValidationException(
                    $"Sprint end date ({sprintEndDate:yyyy-MM-dd}) must be on or before the " +
                    $"project's next milestone due date ({projectData.ExpectedDate:yyyy-MM-dd}).");
            }
        }

        private async Task EnsureProjectExistsAsync(Guid projectId)
        {
            var existence = await _projectService.CheckProjectExistsAsync(projectId);

            switch (existence)
            {
                case ProjectExistence.NotFound:
                    throw new ProjectNotFoundException(projectId);
                case ProjectExistence.Unknown:
                    throw new SprintValidationException(
                        $"Could not verify that project {projectId} exists - Project Service is " +
                        "unavailable or the request was unauthorized.");
                case ProjectExistence.Exists:
                default:
                    return;
            }
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
