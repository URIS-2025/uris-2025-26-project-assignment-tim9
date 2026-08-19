using PaymentService.Models.DTO.InvoiceItemDTOs;

namespace PaymentService.Data
{
    public interface IInvoiceItemRepository
    {
        IEnumerable<InvoiceItemDTO> GetItemsByInvoiceId(Guid invoiceId);
        InvoiceItemDTO? GetItemById(Guid invoiceItemId);
        OperationResult<InvoiceItemConfirmationDTO> AddItem(Guid invoiceId, InvoiceItemCreationDTO item);
        OperationResult<InvoiceItemConfirmationDTO> UpdateItem(Guid invoiceItemId, InvoiceItemUpdateDTO item);
        OperationResult<bool> DeleteItem(Guid invoiceItemId);
    }
}
