using AutoMapper;
using WorkPackageService.Context;
using WorkPackageService.Models.DTO.TaskDTOs;
using Task = WorkPackageService.Models.Task;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Data
{
    public class TaskRepository : ITaskRepository
    {
        private readonly WorkPackageServiceContext _context;
        private readonly IMapper _mapper;

        public TaskRepository(WorkPackageServiceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<TaskDisplayDTO> GetAll()
        {
            var entities = _context.Tasks.ToList();
            return _mapper.Map<IEnumerable<TaskDisplayDTO>>(entities);
        }

        public TaskDisplayDTO? GetById(Guid id)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == id);
            if (entity == null) return null;
            return _mapper.Map<TaskDisplayDTO>(entity);
        }

        public TaskDisplayDTO Add(TaskCreateDTO dto)
        {
            var entity = _mapper.Map<Task>(dto);
            entity.TaskId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _context.Tasks.Add(entity);
            return _mapper.Map<TaskDisplayDTO>(entity);
        }

        public TaskDisplayDTO? Update(Guid id, TaskUpdateDTO dto)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<TaskDisplayDTO>(entity);
        }

        // Task se ne oslanja na FK cascade (Dependency FK-ovi su Restrict) - eksplicitno
        // brisemo sve Dependency zapise gde je ovaj task blokiran ili blokira drugi task.
        public bool Delete(Guid id)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == id);
            if (entity == null) return false;

            var relatedDependencies = _context.Dependencies
                .Where(d => d.TaskId == id || d.BlockerTaskId == id)
                .ToList();
            _context.Dependencies.RemoveRange(relatedDependencies);

            _context.Tasks.Remove(entity);
            return true;
        }

        public IEnumerable<TaskDisplayDTO> GetTasksByWorkPackageId(Guid workPackageId)
        {
            var entities = _context.Tasks.Where(t => t.WorkPackageId == workPackageId).ToList();
            return _mapper.Map<IEnumerable<TaskDisplayDTO>>(entities);
        }

        public IEnumerable<TaskDisplayDTO> GetSubTasks(Guid parentTaskId)
        {
            var entities = _context.Tasks.Where(t => t.ParentTaskId == parentTaskId).ToList();
            return _mapper.Map<IEnumerable<TaskDisplayDTO>>(entities);
        }

        // Autorizacija: status moze da promeni samo osoba kojoj je task dodeljen.
        public TaskDisplayDTO? UpdateStatus(Guid taskId, Guid callerId, TaskStatus newStatus)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (entity == null) return null;
            if (entity.AssigneeId != callerId) return null;

            entity.Status = newStatus;
            entity.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<TaskDisplayDTO>(entity);
        }

        // Premesta task u drugi WorkPackage. Ako task ima Dependency zapise (kao blokirani
        // ili kao blokirajuci), premestanje se i dalje izvrsava, ali se vraca upozorenje
        // umesto tihe izmene - pozivajuci sloj (kontroler) odlucuje kako da ga prikaze.
        public TaskMoveResultDTO? MoveToWorkPackage(Guid taskId, Guid newWorkPackageId)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (entity == null) return null;

            bool hasDependencies = _context.Dependencies
                .Any(d => d.TaskId == taskId || d.BlockerTaskId == taskId);

            entity.WorkPackageId = newWorkPackageId;
            entity.UpdatedAt = DateTime.UtcNow;

            return new TaskMoveResultDTO
            {
                Task = _mapper.Map<TaskDisplayDTO>(entity),
                HasDependencyWarning = hasDependencies,
                Warning = hasDependencies
                    ? "Task ima postojece Dependency zapise - provjeri da premestanje u drugi WorkPackage ne narusava redosled izvrsavanja."
                    : null
            };
        }

        // Vraca i staru i novu vrednost AssigneeId - koristi se za buduce notifikacije.
        public TaskReassignResultDTO? Reassign(Guid taskId, Guid newAssigneeId)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (entity == null) return null;

            var oldAssigneeId = entity.AssigneeId;
            entity.AssigneeId = newAssigneeId;
            entity.UpdatedAt = DateTime.UtcNow;

            return new TaskReassignResultDTO
            {
                Task = _mapper.Map<TaskDisplayDTO>(entity),
                OldAssigneeId = oldAssigneeId,
                NewAssigneeId = newAssigneeId
            };
        }
    }
}
