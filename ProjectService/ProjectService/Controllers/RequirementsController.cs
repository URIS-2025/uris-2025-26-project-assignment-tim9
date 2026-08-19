using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.RequirementsDtos;

namespace ProjectService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly IRequirementsRepository _requirementsRepository;
        private readonly IMapper _mapper;

        public RequirementsController(IRequirementsRepository requirementsRepository, IMapper mapper)
        {
            _requirementsRepository = requirementsRepository;
            _mapper = mapper;
        }

        // GET svi zahtevi
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<RequirementsDto>> GetRequirements()
        {
            var requirements = _requirementsRepository.GetRequirements();
            if (requirements == null || !requirements.Any())
                return NoContent();
            return Ok(requirements);
        }

        // GET zahtevi po projektu
        [HttpGet("project/{projectId}")]
        public ActionResult<IEnumerable<RequirementsDto>> GetRequirementsByProjectId(Guid projectId)
        {
            var requirements = _requirementsRepository.GetRequirementsByProjectId(projectId);
            if (requirements == null || !requirements.Any())
                return NoContent();
            return Ok(requirements);
        }

        // GET zahtev po ID
        [HttpGet("{requirementId}")]
        public ActionResult<RequirementsDto> GetRequirementById(Guid requirementId)
        {
            var requirement = _requirementsRepository.GetRequirementById(requirementId);
            if (requirement == null)
                return NotFound($"Requirement with ID {requirementId} not found.");
            return Ok(requirement);
        }

        // POST kreiraj zahtev
        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult<RequirementsConfirmationDto> CreateRequirement([FromBody] RequirementsCreationDto requirementDto)
        {
            try
            {
                var requirement = _requirementsRepository.CreateRequirement(requirementDto);
                return Created("", requirement);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PUT azuriraj zahtev
        [HttpPut]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult<RequirementsConfirmationDto> UpdateRequirement([FromBody] RequirementsUpdateDto requirementDto)
        {
            try
            {
                var updated = _requirementsRepository.UpdateRequirement(requirementDto);
                if (updated == null)
                    return NotFound($"Requirement with ID {requirementDto.RequirementId} not found.");
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi zahtev
        [HttpDelete("{requirementId}")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public IActionResult DeleteRequirement(Guid requirementId)
        {
            try
            {
                var requirement = _requirementsRepository.GetRequirementById(requirementId);
                if (requirement == null)
                    return NotFound($"Requirement with ID {requirementId} not found.");
                _requirementsRepository.DeleteRequirement(requirementId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetRequirementsOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, PUT, DELETE");
            return Ok();
        }
    }
}
