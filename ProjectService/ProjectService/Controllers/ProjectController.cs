using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;

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
        public ActionResult<IEnumerable<ProjectDto>> GetProjectsByStatus(string status)
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

        // GET projekat po ID
        [HttpGet("{ProjectId}")]
        public ActionResult<ProjectDto> GetProjectById(Guid ProjectId)
        {
            var project = _projectRepository.GetProjectById(ProjectId);
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
        public ActionResult<ProjectConfirmationDto> UpdateProject([FromBody] Project project)
        {
            try
            {
                var existing = _projectRepository.GetProjectById(project.ProjectId);
                if (existing == null)
                    return NotFound();
                var updated = _projectRepository.UpdateProject(project);
                return Ok(updated);
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE obrisi projekat
        [HttpDelete("{ProjectId}")]
        public IActionResult DeleteProject(Guid ProjectId)
        {
            try
            {
                var project = _projectRepository.GetProjectById(ProjectId);
                if (project == null)
                    return NotFound();
                _projectRepository.DeleteProject(ProjectId);
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
