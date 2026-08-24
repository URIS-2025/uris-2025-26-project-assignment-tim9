using AutoMapper;
using WorkPackageService.Context;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.WorkPackageDTOs;

namespace WorkPackageService.Data
{
    public class WorkPackageRepository : IWorkPackageRepository
    {
        private readonly WorkPackageServiceContext _context;
        private readonly IMapper _mapper;

        public WorkPackageRepository(WorkPackageServiceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<WorkPackageDisplayDTO> GetAll()
        {
            var entities = _context.WorkPackages.ToList();
            return _mapper.Map<IEnumerable<WorkPackageDisplayDTO>>(entities);
        }

        public IEnumerable<WorkPackageDisplayDTO> GetByProjectId(Guid projectId)
        {
            var entities = _context.WorkPackages.Where(wp => wp.ProjectId == projectId).ToList();
            return _mapper.Map<IEnumerable<WorkPackageDisplayDTO>>(entities);
        }

        public WorkPackageDisplayDTO? GetById(Guid id)
        {
            var entity = _context.WorkPackages.FirstOrDefault(wp => wp.WorkPackageId == id);
            if (entity == null) return null;
            return _mapper.Map<WorkPackageDisplayDTO>(entity);
        }

        public WorkPackageDisplayDTO Add(WorkPackageCreateDTO dto)
        {
            var entity = _mapper.Map<WorkPackage>(dto);
            entity.WorkPackageId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _context.WorkPackages.Add(entity);
            SaveChanges();
            return _mapper.Map<WorkPackageDisplayDTO>(entity);
        }

        public WorkPackageDisplayDTO? Update(Guid id, WorkPackageUpdateDTO dto)
        {
            var entity = _context.WorkPackages.FirstOrDefault(wp => wp.WorkPackageId == id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            SaveChanges();

            return _mapper.Map<WorkPackageDisplayDTO>(entity);
        }

        public bool Delete(Guid id)
        {
            var entity = _context.WorkPackages.FirstOrDefault(wp => wp.WorkPackageId == id);
            if (entity == null) return false;

            _context.WorkPackages.Remove(entity);
            return SaveChanges();
        }
    }
}
