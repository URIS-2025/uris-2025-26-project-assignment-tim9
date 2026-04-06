using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.MilestoneDtos;

namespace ProjectService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IMapper _mapper;

        public MilestoneController(IMilestoneRepository milestoneRepository, IMapper mapper)
        {
            _milestoneRepository = milestoneRepository;
            _mapper = mapper;
        }

        // GET svi milestone-ovi
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<MilestoneDto>> GetMilestones()
        {
            var milestones = _milestoneRepository.GetMilestones();
            if (milestones == null || !milestones.Any())
                return NoContent();
            return Ok(milestones);
        }

        // GET milestone-ovi po projektu
        [HttpGet("project/{ProjectId}")]
        public ActionResult<IEnumerable<MilestoneDto>> GetMilestonesByProjectId(Guid ProjectId)
        {
            var milestones = _milestoneRepository.GetMilestonesByProjectId(ProjectId);
            if (milestones == null || !milestones.Any())
                return NoContent();
            return Ok(milestones);
        }

        // GET milestone po ID
        [HttpGet("{MilestoneId}")]
        public ActionResult<MilestoneDto> GetMilestoneById(Guid MilestoneId)
        {
            var milestone = _milestoneRepository.GetMilestoneById(MilestoneId);
            if (milestone == null)
                return NotFound();
            return Ok(milestone);
        }

        // POST kreiraj milestone
        [HttpPost]
        public ActionResult<MilestoneConfirmationDto> CreateMilestone([FromBody] MilestoneCreationDto milestoneDto)
        {
            try
            {
                var milestone = _milestoneRepository.CreateMilestone(milestoneDto);
                return Created("", milestone);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PUT azuriraj milestone
        [HttpPut]
        public ActionResult<MilestoneConfirmationDto> UpdateMilestone([FromBody] Milestone milestone)
        {
            try
            {
                var existing = _milestoneRepository.GetMilestoneById(milestone.MilestoneId);
                if (existing == null)
                    return NotFound();
                var updated = _milestoneRepository.UpdateMilestone(milestone);
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi milestone
        [HttpDelete("{MilestoneId}")]
        public IActionResult DeleteMilestone(Guid MilestoneId)
        {
            try
            {
                var milestone = _milestoneRepository.GetMilestoneById(MilestoneId);
                if (milestone == null)
                    return NotFound();
                _milestoneRepository.DeleteMilestone(MilestoneId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetMilestoneOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, PUT, DELETE");
            return Ok();
        }
    }
}
