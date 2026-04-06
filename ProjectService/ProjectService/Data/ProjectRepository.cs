using AutoMapper;
using ProjectService.Context;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
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

        public IEnumerable<ProjectDto> GetProjectsByStatus(string status)
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

        public ProjectDto GetProjectById(Guid ProjectId)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == ProjectId);
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
                Status = projectDto.Status
            };

            _context.Projects.Add(project);
            _context.SaveChanges();

            return new ProjectConfirmationDto
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Budget = project.Budget,
                Status = project.Status
            };
        }

        public ProjectConfirmationDto UpdateProject(Project project)
        {
            var existing = _context.Projects.FirstOrDefault(p => p.ProjectId == project.ProjectId);

            if (existing != null)
            {
                existing.Name = project.Name;
                existing.Budget = project.Budget;
                existing.Status = project.Status;
                _context.SaveChanges();
            }

            return new ProjectConfirmationDto
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Budget = project.Budget,
                Status = project.Status
            };
        }

        public void DeleteProject(Guid ProjectId)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == ProjectId);
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
    }
}

