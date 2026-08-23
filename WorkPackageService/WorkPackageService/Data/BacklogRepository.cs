using AutoMapper;
using WorkPackageService.Context;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.BacklogDTOs;

namespace WorkPackageService.Data
{
    public class BacklogRepository : IBacklogRepository
    {
        private readonly WorkPackageServiceContext _context;
        private readonly IMapper _mapper;

        public BacklogRepository(WorkPackageServiceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<BacklogDisplayDTO> GetAll()
        {
            var entities = _context.Backlogs.ToList();
            return _mapper.Map<IEnumerable<BacklogDisplayDTO>>(entities);
        }

        public IEnumerable<BacklogDisplayDTO> GetByProjectId(Guid projectId)
        {
            var entities = _context.Backlogs.Where(b => b.ProjectId == projectId).ToList();
            return _mapper.Map<IEnumerable<BacklogDisplayDTO>>(entities);
        }

        public BacklogDisplayDTO? GetById(Guid id)
        {
            var entity = _context.Backlogs.FirstOrDefault(b => b.BacklogId == id);
            if (entity == null) return null;
            return _mapper.Map<BacklogDisplayDTO>(entity);
        }

        public BacklogDisplayDTO Add(BacklogCreateDTO dto)
        {
            var entity = _mapper.Map<Backlog>(dto);
            entity.BacklogId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _context.Backlogs.Add(entity);
            SaveChanges();
            return _mapper.Map<BacklogDisplayDTO>(entity);
        }

        public BacklogDisplayDTO? Update(Guid id, BacklogUpdateDTO dto)
        {
            var entity = _context.Backlogs.FirstOrDefault(b => b.BacklogId == id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            SaveChanges();

            return _mapper.Map<BacklogDisplayDTO>(entity);
        }

        public bool Delete(Guid id)
        {
            var entity = _context.Backlogs.FirstOrDefault(b => b.BacklogId == id);
            if (entity == null) return false;

            _context.Backlogs.Remove(entity);
            return SaveChanges();
        }
    }
}
