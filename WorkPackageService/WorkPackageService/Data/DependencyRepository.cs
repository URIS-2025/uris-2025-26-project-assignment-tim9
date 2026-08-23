using AutoMapper;
using WorkPackageService.Context;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.DependencyDTOs;

namespace WorkPackageService.Data
{
    public class DependencyRepository : IDependencyRepository
    {
        private readonly WorkPackageServiceContext _context;
        private readonly IMapper _mapper;

        public DependencyRepository(WorkPackageServiceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<DependencyDisplayDTO> GetAll()
        {
            var entities = _context.Dependencies.ToList();
            return _mapper.Map<IEnumerable<DependencyDisplayDTO>>(entities);
        }

  
        public IEnumerable<DependencyDisplayDTO> GetByTaskId(Guid taskId)
        {
            var entities = _context.Dependencies.Where(d => d.TaskId == taskId).ToList();
            return _mapper.Map<IEnumerable<DependencyDisplayDTO>>(entities);
        }

        public DependencyDisplayDTO? GetById(Guid id)
        {
            var entity = _context.Dependencies.FirstOrDefault(d => d.DependencyId == id);
            if (entity == null) return null;
            return _mapper.Map<DependencyDisplayDTO>(entity);
        }

        public DependencyDisplayDTO? Add(DependencyCreateDTO dto)
        {
            // Osnovna provera - task ne moze blokirati sam sebe.
            // Prava validacija (Validation atributi) dolazi u sledecoj fazi.
            if (dto.TaskId == dto.BlockerTaskId) return null;

            var entity = _mapper.Map<Dependency>(dto);
            entity.DependencyId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _context.Dependencies.Add(entity);
            SaveChanges();
            return _mapper.Map<DependencyDisplayDTO>(entity);
        }

        public DependencyDisplayDTO? Update(Guid id, DependencyUpdateDTO dto)
        {
            var entity = _context.Dependencies.FirstOrDefault(d => d.DependencyId == id);
            if (entity == null) return null;

            if (dto.BlockerTaskId.HasValue && dto.BlockerTaskId.Value == entity.TaskId) return null;

            _mapper.Map(dto, entity);
            SaveChanges();

            return _mapper.Map<DependencyDisplayDTO>(entity);
        }

        public bool Delete(Guid id)
        {
            var entity = _context.Dependencies.FirstOrDefault(d => d.DependencyId == id);
            if (entity == null) return false;

            _context.Dependencies.Remove(entity);
            return SaveChanges();
        }
    }
}
