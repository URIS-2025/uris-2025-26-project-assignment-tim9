using Microsoft.AspNetCore.Mvc;
using TimelogService.Data;
using TimelogService.Models;
using TimelogService.Models.DTO;

namespace TimelogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimelogController : ControllerBase
    {
        private readonly ITimelogRepository _timelogRepository;

        //dependency injection
        public TimelogController(ITimelogRepository timelogRepository)
        {
            _timelogRepository = timelogRepository;
        }

        // GET: api/timelog
        [HttpGet]
        public ActionResult<IEnumerable<TimelogDTO>> GetTimelogs()
        {
            var timelogs = _timelogRepository.GetTimelogs();
            if (timelogs == null || !timelogs.Any())
            {
                return NoContent(); // 204 No Content ako je lista prazna
            }
            return Ok(timelogs); // 200 OK
        }

        // GET: api/timelog/{id}
        [HttpGet("{id}")]
        public ActionResult<TimelogDTO> GetTimelogById(Guid id)
        {
            var timelog = _timelogRepository.GetTimelogById(id);
            if (timelog == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(timelog);
        }

        // POST: api/timelog
        [HttpPost]
        public ActionResult<TimelogConfirmationDTO> CreateTimelog([FromBody] TimelogCreationDTO timelog)
        {
            try
            {
                var confirmation = _timelogRepository.CreateTimelog(timelog);

                return Created("", confirmation);
            }
            catch
            {
                return BadRequest(); // 400 Bad Request
            }
        }

        // DELETE: api/timelog/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTimelog(Guid id)
        {
            try
            {
                var existingLog = _timelogRepository.GetTimelogById(id);
                if (existingLog == null)
                {
                    return NotFound();
                }

                _timelogRepository.DeleteTimelog(id);
                return NoContent(); // 204 Success, no content to return
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error while trying to delete.");
            }
        }

        // PUT: api/timelog
        [HttpPut]
        public ActionResult<TimelogConfirmationDTO> UpdateTimelog([FromBody] Timelog timelog)
        {
            try
            {
                var existingLog = _timelogRepository.GetTimelogById(timelog.Id);
                if (existingLog == null)
                {
                    return NotFound();
                }

                var updatedLog = _timelogRepository.UpdateTimelog(timelog);
                return Ok(updatedLog);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}