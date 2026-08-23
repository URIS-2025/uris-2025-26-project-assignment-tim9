using Microsoft.AspNetCore.Mvc;
using TimelogService.Data;
using TimelogService.Exceptions;
using TimelogService.Models.DTO;

namespace TimelogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimelogController : ControllerBase
    {
        private const string UserIdHeaderName = "X-User-Id";

        private readonly ITimelogRepository _timelogRepository;

        //dependency injection
        public TimelogController(ITimelogRepository timelogRepository)
        {
            _timelogRepository = timelogRepository;
        }

        private string? GetBearerToken()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            return authHeader.StartsWith("Bearer ") ? authHeader["Bearer ".Length..] : null;
        }

        private bool TryGetActingUserId(out Guid userId)
        {
            userId = Guid.Empty;
            return Request.Headers.TryGetValue(UserIdHeaderName, out var userIdHeader) &&
                   Guid.TryParse(userIdHeader, out userId);
        }

        // GET: api/timelog
        // GET: api/timelog?projectId={id}&taskId={id}
        [HttpGet]
        public ActionResult<IEnumerable<TimelogDTO>> GetTimelogs([FromQuery] Guid? projectId, [FromQuery] Guid? taskId)
        {
            return Ok(_timelogRepository.GetTimelogs(projectId, taskId));
        }

        // GET: api/timelog/{id}
        [HttpGet("{id:guid}", Name = "GetTimelogById")]
        public ActionResult<TimelogDTO> GetTimelogById(Guid id)
        {
            var timelog = _timelogRepository.GetTimelogById(id);
            if (timelog is null)
            {
                return NotFound();
            }
            return Ok(timelog);
        }

        // POST: api/timelog
        [HttpPost]
        public async Task<ActionResult<TimelogConfirmationDTO>> CreateTimelog(TimelogCreationDTO timelog)
        {
            if (!TryGetActingUserId(out var loggedByUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var confirmation = await _timelogRepository.CreateTimelogAsync(timelog, loggedByUserId, GetBearerToken());
                return CreatedAtRoute("GetTimelogById", new { id = confirmation.Id }, confirmation);
            }
            catch (TaskNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ProjectNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ClientCannotLogTimeException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // DELETE: api/timelog/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTimelog(Guid id)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            if (_timelogRepository.GetTimelogById(id) is null)
            {
                return NotFound();
            }

            try
            {
                await _timelogRepository.DeleteTimelogAsync(id, actingUserId, GetBearerToken());
                return NoContent();
            }
            catch (NotTimelogOwnerException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        // PUT: api/timelog/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TimelogConfirmationDTO>> UpdateTimelog(Guid id, TimelogUpdateDTO timelog)
        {
            if (!TryGetActingUserId(out var actingUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            try
            {
                var updated = await _timelogRepository.UpdateTimelogAsync(id, timelog, actingUserId, GetBearerToken());
                if (updated is null)
                {
                    return NotFound();
                }
                return Ok(updated);
            }
            catch (TaskNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ProjectNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UserNotProjectMemberException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ClientCannotLogTimeException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (NotTimelogOwnerException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }
    }
}
