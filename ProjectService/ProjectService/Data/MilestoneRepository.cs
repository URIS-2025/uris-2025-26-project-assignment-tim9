using AutoMapper;
using ProjectService.Context;
using ProjectService.Models;
using ProjectService.Models.DTO.MilestoneDtos;

namespace ProjectService.Data
{
    public class MilestoneRepository : IMilestoneRepository
    {
        private readonly ProjectContext _context;
        private readonly IMapper _mapper;

        public MilestoneRepository(ProjectContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<MilestoneDto> GetMilestones()
        {
            var milestones = _context.Milestones.ToList();
            var result = new List<MilestoneDto>();

            foreach (var milestone in milestones)
            {
                var dto = _mapper.Map<MilestoneDto>(milestone);
                result.Add(dto);
            }

            return result;
        }

        public IEnumerable<MilestoneDto> GetMilestonesByProjectId(Guid projectId)
        {
            var milestones = _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .ToList();

            var result = new List<MilestoneDto>();
            foreach (var milestone in milestones)
            {
                var dto = _mapper.Map<MilestoneDto>(milestone);
                result.Add(dto);
            }

            return result;
        }

        public MilestoneDto GetMilestoneById(Guid milestoneId)
        {
            var milestone = _context.Milestones
                .FirstOrDefault(m => m.MilestoneId == milestoneId);

            if (milestone == null)
                return null;

            return _mapper.Map<MilestoneDto>(milestone);
        }

        public MilestoneConfirmationDto CreateMilestone(MilestoneCreationDto milestoneDto)
        {
            var milestone = new Milestone
            {
                MilestoneId = Guid.NewGuid(),
                ProjectId = milestoneDto.ProjectId,
                ExpectedDate = milestoneDto.ExpectedDate
            };

            _context.Milestones.Add(milestone);
            _context.SaveChanges();

            return new MilestoneConfirmationDto
            {
                MilestoneId = milestone.MilestoneId,
                ProjectId = milestone.ProjectId,
                ExpectedDate = milestone.ExpectedDate
            };
        }

        public MilestoneConfirmationDto? UpdateMilestone(MilestoneUpdateDto milestoneDto)
        {
            var existing = _context.Milestones
                .FirstOrDefault(m => m.MilestoneId == milestoneDto.MilestoneId);

            if (existing == null)
                return null;

            existing.ProjectId = milestoneDto.ProjectId;
            existing.ExpectedDate = milestoneDto.ExpectedDate;
            _context.SaveChanges();

            return new MilestoneConfirmationDto
            {
                MilestoneId = existing.MilestoneId,
                ProjectId = existing.ProjectId,
                ExpectedDate = existing.ExpectedDate
            };
        }

        public void DeleteMilestone(Guid milestoneId)
        {
            var milestone = _context.Milestones
                .FirstOrDefault(m => m.MilestoneId == milestoneId);

            if (milestone != null)
            {
                _context.Remove(milestone);
                _context.SaveChanges();
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}

