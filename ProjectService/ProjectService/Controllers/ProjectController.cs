using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.Enums;

namespace ProjectService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public ProjectController(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        // GET sve projekte
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<ProjectDto>> GetProjects()
        {
            var projects = _projectRepository.GetProjects();
            if (projects == null || !projects.Any())
                return NoContent();
            return Ok(projects);
        }

        // GET projekti po statusu
        [HttpGet("status/{status}")]
        public ActionResult<IEnumerable<ProjectDto>> GetProjectsByStatus(ProjectStatus status)
        {
            var projects = _projectRepository.GetProjectsByStatus(status);
            if (projects == null || !projects.Any())
                return NoContent();
            return Ok(projects);
        }

        // GET projekti po clanu
        [HttpGet("member/{memberId}")]
        public ActionResult<IEnumerable<ProjectDto>> GetProjectsByMemberId(Guid memberId)
        {
            var projects = _projectRepository.GetProjectsByMemberId(memberId);
            if (projects == null || !projects.Any())
                return NoContent();
            return Ok(projects);
        }

        // GET projekti po korisniku
        [HttpGet("user/{userId}")]
        public ActionResult<IEnumerable<ProjectDto>> GetProjectsByUserId(Guid userId)
        {
            var projects = _projectRepository.GetProjectsByUserId(userId);
            if (projects == null || !projects.Any())
                return NoContent();
            return Ok(projects);
        }

        // GET projekat po ID
        [HttpGet("{projectId}")]
        public ActionResult<ProjectDto> GetProjectById(Guid projectId)
        {
            var project = _projectRepository.GetProjectById(projectId);
            if (project == null)
                return NotFound();
            return Ok(project);
        }

        // POST kreiraj projekat
        [HttpPost]
        public ActionResult<ProjectConfirmationDto> CreateProject([FromBody] ProjectCreationDto projectDto)
        {
            try
            {
                var project = _projectRepository.CreateProject(projectDto);
                return Created("", project);
            }
            catch
            {
                return BadRequest();
            }
        }

        // PUT azuriraj projekat
        [HttpPut]
        public ActionResult<ProjectConfirmationDto> UpdateProject([FromBody] ProjectUpdateDto projectDto)
        {
            try
            {
                var updated = _projectRepository.UpdateProject(projectDto);
                if (updated == null)
                    return NotFound();
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi projekat
        [HttpDelete("{projectId}")]
        public IActionResult DeleteProject(Guid projectId)
        {
            try
            {
                var project = _projectRepository.GetProjectById(projectId);
                if (project == null)
                    return NotFound();
                _projectRepository.DeleteProject(projectId);
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        // OPTIONS
        [HttpOptions]
        public IActionResult GetProjectOptions()
        {
            Response.Headers.Add("Allow", "GET, POST, PUT, DELETE");
            return Ok();
        }
    }
}
