using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.TaskDTOs;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace WorkPackageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskController(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<TaskDisplayDTO>> GetTasks()
        {
            var tasks = _taskRepository.GetAll();
            if (tasks == null || !tasks.Any())
                return NoContent();
            return Ok(tasks);
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<TaskDisplayDTO> GetTaskById(Guid id)
        {
            var task = _taskRepository.GetById(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [Authorize]
        [HttpGet("workpackage/{workPackageId}")]
        public ActionResult<IEnumerable<TaskDisplayDTO>> GetTasksByWorkPackage(Guid workPackageId)
        {
            var tasks = _taskRepository.GetTasksByWorkPackageId(workPackageId);
            if (tasks == null || !tasks.Any())
                return NoContent();
            return Ok(tasks);
        }

        [Authorize]
        [HttpGet("sprint/{sprintId}")]
        public ActionResult<IEnumerable<TaskDisplayDTO>> GetTasksBySprint(Guid sprintId)
        {
            var tasks = _taskRepository.GetTasksBySprintId(sprintId);
            if (tasks == null || !tasks.Any())
                return NoContent();
            return Ok(tasks);
        }

        [Authorize]
        [HttpGet("parent/{parentTaskId}")]
        public ActionResult<IEnumerable<TaskDisplayDTO>> GetSubTasks(Guid parentTaskId)
        {
            var subTasks = _taskRepository.GetSubTasks(parentTaskId);
            if (subTasks == null || !subTasks.Any())
                return NoContent();
            return Ok(subTasks);
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpPost]
        public ActionResult<TaskDisplayDTO> CreateTask([FromBody] TaskCreateDTO dto)
        {
            try
            {
                var created = _taskRepository.Add(dto);
                return Created("", created);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpPut]
        public ActionResult<TaskDisplayDTO> UpdateTask([FromBody] TaskUpdateDTO dto)
        {
            try
            {
                var updated = _taskRepository.Update(dto.Id, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(Guid id)
        {
            try
            {
                var deleted = _taskRepository.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mySqlEx && mySqlEx.Number == 1451)
            {
                
                return Conflict("Task cannot be deleted beacuse it has depenedencies or subtasks. Delete or move them.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<TaskDisplayDTO>> UpdateTaskStatus(Guid id, [FromQuery] Guid callerId, [FromBody] TaskStatusUpdateRequestDTO dto)
        {
            try
            {
                var updated = await _taskRepository.UpdateStatus(id, callerId, dto.NewStatus);
                return Ok(updated);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedOperationException)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpPatch("{id}/reassign")]
        public async Task<ActionResult<TaskReassignResultDTO>> ReassignTask(Guid id, [FromBody] TaskReassignRequestDTO dto)
        {
            try
            {
                var result = await _taskRepository.Reassign(id, dto.NewAssigneeId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedOperationException)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ProjectManager,Admin")]
        [HttpPatch("{id}/move")]
        public ActionResult<TaskMoveResultDTO> MoveTask(Guid id, [FromBody] TaskMoveRequestDTO dto)
        {
            try
            {
                var result = _taskRepository.MoveToWorkPackage(id, dto.NewWorkPackageId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpOptions]
        public IActionResult GetTaskOptions()
        {
            Response.Headers["Allow"] = "GET, POST, PUT, DELETE, PATCH";
            return Ok();
        }
    }
}