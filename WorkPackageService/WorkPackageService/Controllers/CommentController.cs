using Microsoft.AspNetCore.Mvc;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.CommentDTOs;

namespace WorkPackageService.Controllers
{
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _repository;

        public CommentController(ICommentRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("tasks/{taskId}/comments")]
        public ActionResult<CommentDisplayDTO> CreateComment(Guid taskId, [FromBody] CommentCreateDTO dto)
        {
            // TaskId dolazi iz rute - isti razlog za Clear()+TryValidateModel kao u WorkPackageController.
            dto.TaskId = taskId;
            ModelState.Clear();
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var created = _repository.Add(dto);

            return CreatedAtAction(nameof(GetCommentById), new { id = created.CommentId }, created);
        }

        [HttpGet("tasks/{taskId}/comments")]
        public ActionResult<IEnumerable<CommentDisplayDTO>> GetCommentsForTask(Guid taskId)
        {
            return Ok(_repository.GetByTaskId(taskId));
        }

        [HttpGet("comments/{id}")]
        public ActionResult<CommentDisplayDTO> GetCommentById(Guid id)
        {
            var comment = _repository.GetById(id);
            if (comment == null) return NotFound();

            return Ok(comment);
        }

        // PRIVREMENO: dok ne postoji pravi auth middleware, identitet pozivaoca se prosledjuje
        // kao query parametar (?callerId={guid}). Kad se doda prava autentifikacija, ovo treba
        // zameniti citanjem callerId-a iz autentifikovanog korisnika (npr. iz tokena), ne iz query-ja.
        [HttpPut("comments/{id}")]
        public ActionResult<CommentDisplayDTO> UpdateComment(Guid id, [FromQuery] Guid callerId, [FromBody] CommentUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = _repository.Update(id, callerId, dto);

                return Ok(updated);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedOperationException)
            {
                // Forbid() bi zahtevao registrovanu auth semu (AddAuthentication), koje jos
                // nema u ovom servisu - vracamo plain 403 status kod dok se ne ugradi prava
                // autentifikacija (Forbid() bi inace bacio InvalidOperationException).
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        // PRIVREMENO: isti callerId-preko-query-ja pristup kao kod UpdateComment - vidi napomenu gore.
        [HttpDelete("comments/{id}")]
        public IActionResult DeleteComment(Guid id, [FromQuery] Guid callerId)
        {
            try
            {
                _repository.Delete(id, callerId);

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
        }
    }
}
