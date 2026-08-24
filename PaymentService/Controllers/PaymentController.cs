using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Swagger;
using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;

        //dependency injection
        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        // GET: api/payment
        // GET: api/payment?invoiceId={id}&paidByUserId={id}
        [HttpGet]
        [HttpHead]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<PaymentDTO>> GetPayments([FromQuery] Guid? invoiceId, [FromQuery] Guid? paidByUserId)
        {
            return Ok(_paymentRepository.GetPayments(invoiceId, paidByUserId));
        }

        // GET: api/payment/{paymentId}
        [HttpGet("{paymentId:guid}", Name = "GetPaymentById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PaymentDTO> GetPaymentById(Guid paymentId)
        {
            var payment = _paymentRepository.GetPaymentById(paymentId);

            if (payment is null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        // POST: api/payment
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [RequiresUserId]
        public async Task<ActionResult<PaymentConfirmationDTO>> CreatePayment([FromBody] PaymentCreationDTO payment)
        {
            if (!Request.TryGetUserId(out var paidByUserId))
            {
                return BadRequest($"Nedostaje ili je neispravan {UserHeader.Name} header.");
            }

            var result = await _paymentRepository.CreatePaymentAsync(payment, paidByUserId);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return CreatedAtRoute("GetPaymentById", new { paymentId = result.Value!.PaymentId }, result.Value);
        }

        // PUT: api/payment/{paymentId}
        [HttpPut("{paymentId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PaymentConfirmationDTO>> UpdatePayment(Guid paymentId, [FromBody] PaymentUpdateDTO payment)
        {
            var result = await _paymentRepository.UpdatePaymentAsync(paymentId, payment);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return Ok(result.Value);
        }

        // DELETE: api/payment/{paymentId}
        [HttpDelete("{paymentId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeletePayment(Guid paymentId)
        {
            var result = _paymentRepository.DeletePayment(paymentId);

            if (!result.IsSuccess)
            {
                return OutcomeMapper.ToResponse(result.Outcome);
            }

            return NoContent();
        }

        // OPTIONS: api/payment
        [HttpOptions]
        public IActionResult GetPaymentOptions()
        {
            Response.Headers.Append("Allow", "GET, HEAD, POST, PUT, DELETE, OPTIONS");
            return Ok();
        }
    }
}
