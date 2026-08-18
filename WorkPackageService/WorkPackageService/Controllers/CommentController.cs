using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.CommentDTOs;

namespace WorkPackageService.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentController(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<CommentDisplayDTO>> GetComments()
        {
            var comments = _commentRepository.GetAll();
            if (comments == null || !comments.Any())
                return NoContent();
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public ActionResult<CommentDisplayDTO> GetCommentById(Guid id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpGet("task/{taskId}")]
        public ActionResult<IEnumerable<CommentDisplayDTO>> GetCommentsByTask(Guid taskId)
        {
            var comments = _commentRepository.GetByTaskId(taskId);
            if (comments == null || !comments.Any())
                return NoContent();
            return Ok(comments);
        }

        [HttpPost]
        public ActionResult<CommentDisplayDTO> CreateComment([FromBody] CommentCreateDTO dto)
        {
            try
            {
                var created = _commentRepository.Add(dto);
                return Created("", created);
            }
            catch
            {
                return BadRequest();
            }
        }

  
        [HttpPut]
        public ActionResult<CommentDisplayDTO> UpdateComment([FromQuery] Guid callerId, [FromBody] CommentUpdateDTO dto)
        {
            try
            {
                var updated = _commentRepository.Update(dto.Id, callerId, dto);
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

        [HttpDelete("{id}")]
        public IActionResult DeleteComment(Guid id, [FromQuery] Guid callerId)
        {
            try
            {
                _commentRepository.Delete(id, callerId);
                return NoContent();
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
                return StatusCode(StatusCodes.Status500InternalServerError, "Delete Error");
            }
        }

        [HttpOptions]
        public IActionResult GetCommentOptions()
        {
            Response.Headers["Allow"] = "GET, POST, PUT, DELETE";
            return Ok();
        }
    }
}
