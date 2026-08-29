using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Data
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<InvoiceDTO>> GetInvoicesAsync(Guid callerId, bool isAdmin, Guid? projectId = null, InvoiceStatus? status = null);
        InvoiceDTO? GetInvoiceById(Guid invoiceId);
        Task<OperationResult<InvoiceConfirmationDTO>> CreateInvoiceAsync(InvoiceCreationDTO invoice, Guid issuedByUserId, bool isAdmin);
        Task<OperationResult<InvoiceConfirmationDTO>> UpdateInvoiceAsync(Guid invoiceId, InvoiceUpdateDTO invoice);
        OperationResult<bool> DeleteInvoice(Guid invoiceId);
    }
}
