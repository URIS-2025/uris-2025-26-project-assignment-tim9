using WorkPackageService.Models.DTO.TaskDTOs;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Data
{
    public interface ITaskRepository
    {
        IEnumerable<TaskDisplayDTO> GetAll();
        TaskDisplayDTO? GetById(Guid id);
        TaskDisplayDTO Add(TaskCreateDTO dto);
        TaskDisplayDTO? Update(Guid id, TaskUpdateDTO dto);
        bool Delete(Guid id);
        bool SaveChanges();

        IEnumerable<TaskDisplayDTO> GetTasksByWorkPackageId(Guid workPackageId);
        IEnumerable<TaskDisplayDTO> GetSubTasks(Guid parentTaskId);
        TaskDisplayDTO? UpdateStatus(Guid taskId, Guid callerId, TaskStatus newStatus);
        TaskMoveResultDTO? MoveToWorkPackage(Guid taskId, Guid newWorkPackageId);
        TaskReassignResultDTO? Reassign(Guid taskId, Guid newAssigneeId);
    }
}
