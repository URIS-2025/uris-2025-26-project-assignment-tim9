using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Swagger;
using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;

        //dependency injection
        public InvoiceController(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        // GET: api/invoice
        // GET: api/invoice?projectId={id}&status=Unpaid
        [HttpGet]
        [HttpHead]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<InvoiceDTO>> GetInvoices([FromQuery] Guid? projectId, [FromQuery] InvoiceStatus? status)
        {
            return Ok(_invoiceRepository.GetInvoices(projectId, status));
        }

        // GET: api/invoice/{invoiceId}
        [HttpGet("{invoiceId:guid}", Name = "GetInvoiceById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<InvoiceDTO> GetInvoiceById(Guid invoiceId)
        {
            var invoice = _invoiceRepository.GetInvoiceById(invoiceId);

            if (invoice is null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        // POST: api/invoice
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiresUserId]
        public async Task<ActionResult<InvoiceConfirmationDTO>> CreateInvoice([FromBody] InvoiceCreationDTO invoice)
        {
            if (!Request.TryGetUserId(out var issuedByUserId))
            {
                return BadRequest($"Nedostaje ili je neispravan {UserHeader.Name} header.");
            }

            var confirmation = await _invoiceRepository.CreateInvoiceAsync(invoice, issuedByUserId);

            return CreatedAtRoute("GetInvoiceById", new { invoiceId = confirmation.InvoiceId }, confirmation);
        }

        // PUT: api/invoice/{invoiceId}
        [HttpPut("{invoiceId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<InvoiceConfirmationDTO>> UpdateInvoice(Guid invoiceId, [FromBody] InvoiceUpdateDTO invoice)
        {
            var result = await _invoiceRepository.UpdateInvoiceAsync(invoiceId, invoice);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return Ok(result.Value);
        }

        // DELETE: api/invoice/{invoiceId}
        [HttpDelete("{invoiceId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult DeleteInvoice(Guid invoiceId)
        {
            var result = _invoiceRepository.DeleteInvoice(invoiceId);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return NoContent();
        }

        // OPTIONS: api/invoice
        [HttpOptions]
        public IActionResult GetInvoiceOptions()
        {
            Response.Headers.Append("Allow", "GET, HEAD, POST, PUT, DELETE, OPTIONS");
            return Ok();
        }
    }
}
