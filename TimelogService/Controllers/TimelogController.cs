using Microsoft.AspNetCore.Mvc;
using TimelogService.Data;
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

        // GET: api/timelog
        // GET: api/timelog?projectId={id}&workPackageId={id}
        [HttpGet]
        public ActionResult<IEnumerable<TimelogDTO>> GetTimelogs([FromQuery] Guid? projectId, [FromQuery] Guid? workPackageId)
        {
            return Ok(_timelogRepository.GetTimelogs(projectId, workPackageId));
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
            if (!Request.Headers.TryGetValue(UserIdHeaderName, out var userIdHeader) ||
                !Guid.TryParse(userIdHeader, out var loggedByUserId))
            {
                return BadRequest($"Missing or invalid {UserIdHeaderName} header - this is expected to be set by the API Gateway after authenticating the caller.");
            }

            var confirmation = await _timelogRepository.CreateTimelogAsync(timelog, loggedByUserId);
            return CreatedAtRoute("GetTimelogById", new { id = confirmation.Id }, confirmation);
        }

        // DELETE: api/timelog/{id}
        [HttpDelete("{id:guid}")]
        public IActionResult DeleteTimelog(Guid id)
        {
            if (_timelogRepository.GetTimelogById(id) is null)
            {
                return NotFound();
            }

            _timelogRepository.DeleteTimelog(id);
            return NoContent();
        }

        // PUT: api/timelog/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TimelogConfirmationDTO>> UpdateTimelog(Guid id, TimelogUpdateDTO timelog)
        {
            var updated = await _timelogRepository.UpdateTimelogAsync(id, timelog);
            if (updated is null)
            {
                return NotFound();
            }
            return Ok(updated);
        }
    }
}
