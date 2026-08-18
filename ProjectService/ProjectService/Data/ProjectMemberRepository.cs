using AutoMapper;
using ProjectService.Context;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectMemberDtos;

namespace ProjectService.Data
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly ProjectContext _context;
        private readonly IMapper _mapper;

        public ProjectMemberRepository(ProjectContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<ProjectMemberDto> GetProjectMembers()
        {
            var members = _context.ProjectMembers.ToList();
            var result = new List<ProjectMemberDto>();

            foreach (var member in members)
            {
                var dto = _mapper.Map<ProjectMemberDto>(member);
                result.Add(dto);
            }

            return result;
        }

        public IEnumerable<ProjectMemberDto> GetMembersByProjectId(Guid ProjectId)
        {
            var members = _context.ProjectMembers
                .Where(pm => pm.ProjectId == ProjectId)
                .ToList();

            var result = new List<ProjectMemberDto>();
            foreach (var member in members)
            {
                var dto = _mapper.Map<ProjectMemberDto>(member);
                result.Add(dto);
            }

            return result;
        }

        public ProjectMemberDto GetProjectMemberById(Guid ProjectMemberId)
        {
            var member = _context.ProjectMembers
                .FirstOrDefault(pm => pm.ProjectMemberId == ProjectMemberId);

            if (member == null)
                return null;

            return _mapper.Map<ProjectMemberDto>(member);
        }

        public ProjectMemberConfirmationDto CreateProjectMember(ProjectMemberCreationDto projectMemberDto)
        {
            var member = new ProjectMember
            {
                ProjectMemberId = Guid.NewGuid(),
                ProjectId = projectMemberDto.ProjectId,
                UserId = projectMemberDto.UserId,
                JoinedAt = DateTime.UtcNow,
                Status = true
            };

            _context.ProjectMembers.Add(member);
            _context.SaveChanges();

            return new ProjectMemberConfirmationDto
            {
                ProjectMemberId = member.ProjectMemberId,
                ProjectId = member.ProjectId,
                UserId = projectMemberDto.UserId,
                JoinedAt = member.JoinedAt,
                Status = member.Status
            };
        }

        public ProjectMemberConfirmationDto UpdateProjectMember(ProjectMemberUpdateDto projectMemberDto)
        {
            var existing = _context.ProjectMembers
                .FirstOrDefault(pm => pm.ProjectMemberId == projectMemberDto.ProjectMemberId);
            if (existing != null)
            {
                existing.ProjectId = projectMemberDto.ProjectId;
                existing.UserId = projectMemberDto.UserId;
                existing.JoinedAt = projectMemberDto.JoinedAt;
                existing.Status = projectMemberDto.Status;
                _context.SaveChanges();
            }
            return new ProjectMemberConfirmationDto
            {
                ProjectMemberId = projectMemberDto.ProjectMemberId,
                ProjectId = projectMemberDto.ProjectId,
                UserId = projectMemberDto.UserId,
                JoinedAt = projectMemberDto.JoinedAt,
                Status = projectMemberDto.Status
            };
        }

        public void DeleteProjectMember(Guid ProjectMemberId)
        {
            var member = _context.ProjectMembers
                .FirstOrDefault(pm => pm.ProjectMemberId == ProjectMemberId);

            if (member != null)
            {
                _context.Remove(member);
                _context.SaveChanges();
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}

