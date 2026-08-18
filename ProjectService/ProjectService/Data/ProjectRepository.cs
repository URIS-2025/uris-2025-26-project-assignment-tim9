using AutoMapper;
using ProjectService.Context;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.Enums;
//using ProjectService.ServiceCalls.User;

namespace ProjectService.Data
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectContext _context;
        private readonly IMapper _mapper;

        public ProjectRepository(ProjectContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<ProjectDto> GetProjects()
        {
            var projects = _context.Projects.ToList();
            var result = new List<ProjectDto>();

            foreach (var project in projects)
            {
                var dto = _mapper.Map<ProjectDto>(project);
                result.Add(dto);
            }

            return result;
        }

        public IEnumerable<ProjectDto> GetProjectsByStatus(ProjectStatus status)
        {
            var projects = _context.Projects.Where(p => p.Status == status).ToList();
            var result = new List<ProjectDto>();

            foreach (var project in projects)
            {
                var dto = _mapper.Map<ProjectDto>(project);
                result.Add(dto);
            }

            return result;
        }

        public ProjectDto GetProjectById(Guid projectId)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
                return null;

            return _mapper.Map<ProjectDto>(project);
        }

        public ProjectConfirmationDto CreateProject(ProjectCreationDto projectDto)
        {
            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = projectDto.Name,
                Budget = projectDto.Budget,
                Status = projectDto.Status!.Value,
                Deadline = projectDto.Deadline,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            _context.SaveChanges();

            return new ProjectConfirmationDto
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Budget = project.Budget,
                Status = project.Status,
                Deadline = project.Deadline,
                CreatedAt = project.CreatedAt
            };
        }

        public ProjectConfirmationDto? UpdateProject(ProjectUpdateDto projectDto)
        {
            var existing = _context.Projects.FirstOrDefault(p => p.ProjectId == projectDto.ProjectId);

            if (existing == null)
                return null;

            existing.Name = projectDto.Name;
            existing.Budget = projectDto.Budget;
            existing.Status = projectDto.Status!.Value;
            existing.Deadline = projectDto.Deadline;
            _context.SaveChanges();

            return new ProjectConfirmationDto
            {
                ProjectId = existing.ProjectId,
                Name = existing.Name,
                Budget = existing.Budget,
                Status = existing.Status,
                Deadline = existing.Deadline,
                CreatedAt = existing.CreatedAt
            };
        }

        public void DeleteProject(Guid projectId)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project != null)
            {
                _context.Remove(project);
                _context.SaveChanges();
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<ProjectDto> GetProjectsByMemberId(Guid memberId)
        {
            var ProjectIds = _context.ProjectMembers
                .Where(pm => pm.ProjectMemberId == memberId)
                .Select(pm => pm.ProjectId)
                .ToList();

            var projects = _context.Projects
                .Where(p => ProjectIds.Contains(p.ProjectId))
                .ToList();

            var result = new List<ProjectDto>();
            foreach (var project in projects)
            {
                var dto = _mapper.Map<ProjectDto>(project);
                result.Add(dto);
            }

            return result;
        }

        public IEnumerable<ProjectDto> GetProjectsByUserId(Guid userId)
        {
            var projectIds = _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.ProjectId)
                .ToList();

            var projects = _context.Projects
                .Where(p => projectIds.Contains(p.ProjectId))
                .ToList();

            var result = new List<ProjectDto>();
            foreach (var project in projects)
            {
                var dto = _mapper.Map<ProjectDto>(project);
                result.Add(dto);
            }

            return result;
        }
    }
}

