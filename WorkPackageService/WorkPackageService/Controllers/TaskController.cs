using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.TaskDTOs;

namespace WorkPackageService.Controllers
{
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _repository;

        public TaskController(ITaskRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("work-packages/{workPackageId}/tasks")]
        public ActionResult<TaskDisplayDTO> CreateTask(Guid workPackageId, [FromBody] TaskCreateDTO dto)
        {
            // WorkPackageId dolazi iz rute - isti razlog za Clear()+TryValidateModel kao u ostalim kontrolerima.
            dto.WorkPackageId = workPackageId;
            ModelState.Clear();
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var created = _repository.Add(dto);

            return CreatedAtAction(nameof(GetTaskById), new { id = created.TaskId }, created);
        }

        [HttpGet("work-packages/{workPackageId}/tasks")]
        public ActionResult<IEnumerable<TaskDisplayDTO>> GetTasksForWorkPackage(Guid workPackageId)
        {
            return Ok(_repository.GetTasksByWorkPackageId(workPackageId));
        }

        [HttpGet("tasks/{id}")]
        public ActionResult<TaskDisplayDTO> GetTaskById(Guid id)
        {
            var task = _repository.GetById(id);
            if (task == null) return NotFound();

            return Ok(task);
        }

        [HttpPut("tasks/{id}")]
        public ActionResult<TaskDisplayDTO> UpdateTask(Guid id, [FromBody] TaskUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = _repository.Update(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpDelete("tasks/{id}")]
        public IActionResult DeleteTask(Guid id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPost("tasks/{parentTaskId}/subtasks")]
        public ActionResult<TaskDisplayDTO> CreateSubTask(Guid parentTaskId, [FromBody] TaskCreateDTO dto)
        {
            // Isti CreateDTO kao obican task - samo se ParentTaskId popuni iz rute.
            // WorkPackageId i dalje dolazi iz tela zahteva (ne nasledjuje se automatski od parenta).
            dto.ParentTaskId = parentTaskId;
            ModelState.Clear();
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var created = _repository.Add(dto);

            return CreatedAtAction(nameof(GetTaskById), new { id = created.TaskId }, created);
        }

        [HttpGet("tasks/{parentTaskId}/subtasks")]
        public ActionResult<IEnumerable<TaskDisplayDTO>> GetSubTasks(Guid parentTaskId)
        {
            return Ok(_repository.GetSubTasks(parentTaskId));
        }

        // PRIVREMENO: callerId kao query parametar dok ne postoji pravi auth middleware -
        // isti pristup kao u CommentController.
        [HttpPatch("tasks/{id}/status")]
        public async Task<ActionResult<TaskDisplayDTO>> UpdateTaskStatus(Guid id, [FromQuery] Guid callerId, [FromBody] TaskStatusUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = await _repository.UpdateStatus(id, callerId, dto.NewStatus);

                return Ok(updated);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedOperationException)
            {
                // Forbid() bi zahtevao registrovanu auth semu, koje jos nema - vracamo plain 403.
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        [HttpPatch("tasks/{id}/reassign")]
        public async Task<ActionResult<TaskReassignResultDTO>> ReassignTask(Guid id, [FromBody] TaskReassignRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repository.Reassign(id, dto.NewAssigneeId);
            if (result == null) return NotFound();

            // OldAssigneeId/NewAssigneeId iz rezultata koristice sledeca faza za notifikacije.
            return Ok(result);
        }

        [HttpPatch("tasks/{id}/move")]
        public ActionResult<TaskMoveResultDTO> MoveTask(Guid id, [FromBody] TaskMoveRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = _repository.MoveToWorkPackage(id, dto.NewWorkPackageId);
            if (result == null) return NotFound();

            // 200 OK i kad je HasDependencyWarning true - premestanje se izvrsava,
            // upozorenje je samo informativno i ne blokira operaciju.
            return Ok(result);
        }
    }
}
