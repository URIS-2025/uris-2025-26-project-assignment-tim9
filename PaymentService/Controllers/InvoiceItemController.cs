using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Models.DTO.InvoiceItemDTOs;

namespace PaymentService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoiceItemController : ControllerBase
    {
        private readonly IInvoiceItemRepository _invoiceItemRepository;

        //dependency injection
        public InvoiceItemController(IInvoiceItemRepository invoiceItemRepository)
        {
            _invoiceItemRepository = invoiceItemRepository;
        }

        // GET: api/invoice/{invoiceId}/items
        [HttpGet("~/api/invoice/{invoiceId:guid}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<InvoiceItemDTO>> GetItemsByInvoiceId(Guid invoiceId)
        {
            return Ok(_invoiceItemRepository.GetItemsByInvoiceId(invoiceId));
        }

        // GET: api/invoiceitem/{invoiceItemId}
        [HttpGet("{invoiceItemId:guid}", Name = "GetInvoiceItemById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<InvoiceItemDTO> GetItemById(Guid invoiceItemId)
        {
            var item = _invoiceItemRepository.GetItemById(invoiceItemId);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // POST: api/invoice/{invoiceId}/items
        [HttpPost("~/api/invoice/{invoiceId:guid}/items")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult<InvoiceItemConfirmationDTO> AddItem(Guid invoiceId, [FromBody] InvoiceItemCreationDTO item)
        {
            var result = _invoiceItemRepository.AddItem(invoiceId, item);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return CreatedAtRoute("GetInvoiceItemById", new { invoiceItemId = result.Value!.InvoiceItemId }, result.Value);
        }

        // PUT: api/invoiceitem/{invoiceItemId}
        [HttpPut("{invoiceItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult<InvoiceItemConfirmationDTO> UpdateItem(Guid invoiceItemId, [FromBody] InvoiceItemUpdateDTO item)
        {
            var result = _invoiceItemRepository.UpdateItem(invoiceItemId, item);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return Ok(result.Value);
        }

        // DELETE: api/invoiceitem/{invoiceItemId}
        [HttpDelete("{invoiceItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "Admin,ProjectManager")]
        public IActionResult DeleteItem(Guid invoiceItemId)
        {
            var result = _invoiceItemRepository.DeleteItem(invoiceItemId);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return NoContent();
        }

        // OPTIONS: api/invoiceitem
        [HttpOptions]
        public IActionResult GetInvoiceItemOptions()
        {
            Response.Headers.Append("Allow", "GET, POST, PUT, DELETE, OPTIONS");
            return Ok();
        }
    }
}
