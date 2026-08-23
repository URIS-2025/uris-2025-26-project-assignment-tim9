using AutoMapper;
using WorkPackageService.Context;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.TaskDTOs;
using WorkPackageService.ServiceCalls.Notification;
using Task = WorkPackageService.Models.Task;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Data
{
    public class TaskRepository : ITaskRepository
    {
        private readonly WorkPackageServiceContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public TaskRepository(WorkPackageServiceContext context, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
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
            SaveChanges();
            return _mapper.Map<TaskDisplayDTO>(entity);
        }

        public TaskDisplayDTO? Update(Guid id, TaskUpdateDTO dto)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            SaveChanges();

            return _mapper.Map<TaskDisplayDTO>(entity);
        }

      
        public bool Delete(Guid id)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == id);
            if (entity == null) return false;

            var relatedDependencies = _context.Dependencies
                .Where(d => d.TaskId == id || d.BlockerTaskId == id)
                .ToList();
            _context.Dependencies.RemoveRange(relatedDependencies);

            _context.Tasks.Remove(entity);
            return SaveChanges();
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

        public async System.Threading.Tasks.Task<TaskDisplayDTO?> UpdateStatus(Guid taskId, Guid callerId, TaskStatus newStatus)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (entity == null) throw new EntityNotFoundException($"Task sa Id-jem {taskId} ne postoji.");
            if (entity.AssigneeId != callerId) throw new UnauthorizedOperationException("Samo osoba kojoj je task dodeljen moze da promeni njegov status.");

            entity.Status = newStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            SaveChanges();

            if (newStatus == TaskStatus.Done)
            {
                
                var unblocked = _context.Dependencies
                    .Where(d => d.BlockerTaskId == taskId)
                    .Join(_context.Tasks, d => d.TaskId, t => t.TaskId, (d, t) => new { t.TaskId, t.AssigneeId })
                    .ToList();

                foreach (var unblockedTask in unblocked)
                {
                    if (unblockedTask.AssigneeId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            unblockedTask.AssigneeId.Value,
                            $"Task {unblockedTask.TaskId} je odblokiran zavrsetkom taska {taskId}.",
                            "TaskUnblocked");
                    }
                }
            }

            return _mapper.Map<TaskDisplayDTO>(entity);
        }

      
        public TaskMoveResultDTO? MoveToWorkPackage(Guid taskId, Guid newWorkPackageId)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (entity == null) return null;

            bool hasDependencies = _context.Dependencies
                .Any(d => d.TaskId == taskId || d.BlockerTaskId == taskId);

            entity.WorkPackageId = newWorkPackageId;
            entity.UpdatedAt = DateTime.UtcNow;
            SaveChanges();

            return new TaskMoveResultDTO
            {
                Task = _mapper.Map<TaskDisplayDTO>(entity),
                HasDependencyWarning = hasDependencies,
                Warning = hasDependencies
                    ? "Task ima postojece Dependency zapise - provjeri da premestanje u drugi WorkPackage ne narusava redosled izvrsavanja."
                    : null
            };
        }

        
        public async System.Threading.Tasks.Task<TaskReassignResultDTO?> Reassign(Guid taskId, Guid newAssigneeId)
        {
            var entity = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (entity == null) return null;

            var oldAssigneeId = entity.AssigneeId;
            entity.AssigneeId = newAssigneeId;
            entity.UpdatedAt = DateTime.UtcNow;
            SaveChanges();

            if (oldAssigneeId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    oldAssigneeId.Value,
                    $"Vise nisi zaduzen za task {taskId}.",
                    "TaskReassignedFrom");
            }

            await _notificationService.SendNotificationAsync(
                newAssigneeId,
                $"Zaduzen si za task {taskId}.",
                "TaskReassignedTo");

            return new TaskReassignResultDTO
            {
                Task = _mapper.Map<TaskDisplayDTO>(entity),
                OldAssigneeId = oldAssigneeId,
                NewAssigneeId = newAssigneeId
            };
        }
    }
}
