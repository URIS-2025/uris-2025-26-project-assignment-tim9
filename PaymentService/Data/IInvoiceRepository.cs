using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Data
{
    public interface IInvoiceRepository
    {
        IEnumerable<InvoiceDTO> GetInvoices(Guid? projectId = null, InvoiceStatus? status = null);
        InvoiceDTO? GetInvoiceById(Guid invoiceId);
        InvoiceConfirmationDTO CreateInvoice(InvoiceCreationDTO invoice, Guid issuedByUserId);
        OperationResult<InvoiceConfirmationDTO> UpdateInvoice(Guid invoiceId, InvoiceUpdateDTO invoice);
        OperationResult<bool> DeleteInvoice(Guid invoiceId);
    }
}
