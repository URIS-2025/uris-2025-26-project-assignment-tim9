using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;

namespace PaymentService.Controllers
{
    //prevodi ishode poslovnih pravila u HTTP odgovore
    public static class OutcomeMapper
    {
        public static ActionResult ToResponse(OperationOutcome outcome)
        {
            return outcome switch
            {
                OperationOutcome.NotFound =>
                    new NotFoundResult(),

                OperationOutcome.InvoiceIsPaid =>
                    new ConflictObjectResult("Faktura je vec placena i ne moze da se menja."),

                OperationOutcome.InvoiceIsCancelled =>
                    new ConflictObjectResult("Faktura je stornirana."),

                OperationOutcome.AmountExceedsRemainingDebt =>
                    new ConflictObjectResult("Iznos uplate premasuje preostali dug po fakturi."),

                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
