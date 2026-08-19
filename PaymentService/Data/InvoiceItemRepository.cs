using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PaymentService.Context;
using PaymentService.Models;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Data
{
    public class InvoiceItemRepository : IInvoiceItemRepository
    {
        private readonly PaymentContext _context;
        private readonly IMapper _mapper;

        public InvoiceItemRepository(PaymentContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<InvoiceItemDTO> GetItemsByInvoiceId(Guid invoiceId)
        {
            var items = _context.InvoiceItems
                .Where(s => s.InvoiceId == invoiceId)
                .ToList();

            return _mapper.Map<List<InvoiceItemDTO>>(items);
        }

        public InvoiceItemDTO? GetItemById(Guid invoiceItemId)
        {
            var item = _context.InvoiceItems.FirstOrDefault(s => s.InvoiceItemId == invoiceItemId);
            return item is null ? null : _mapper.Map<InvoiceItemDTO>(item);
        }

        public OperationResult<InvoiceItemConfirmationDTO> AddItem(Guid invoiceId, InvoiceItemCreationDTO item)
        {
            var invoice = LoadInvoice(invoiceId);

            if (invoice is null)
            {
                return OperationResult<InvoiceItemConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            var blocked = CheckInvoiceEditable(invoice);
            if (blocked.HasValue)
            {
                return OperationResult<InvoiceItemConfirmationDTO>.Fail(blocked.Value);
            }

            var newItem = _mapper.Map<InvoiceItem>(item);
            newItem.InvoiceItemId = Guid.NewGuid();
            newItem.InvoiceId = invoiceId;
            newItem.TotalAmount = newItem.UnitPrice * newItem.Quantity;

            invoice.Items.Add(newItem);
            RecalculateInvoice(invoice);
            _context.SaveChanges();

            return OperationResult<InvoiceItemConfirmationDTO>.Ok(BuildConfirmation(newItem, invoice));
        }

        public OperationResult<InvoiceItemConfirmationDTO> UpdateItem(Guid invoiceItemId, InvoiceItemUpdateDTO item)
        {
            var existing = _context.InvoiceItems.FirstOrDefault(s => s.InvoiceItemId == invoiceItemId);

            if (existing is null)
            {
                return OperationResult<InvoiceItemConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            var invoice = LoadInvoice(existing.InvoiceId);

            if (invoice is null)
            {
                return OperationResult<InvoiceItemConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            var blocked = CheckInvoiceEditable(invoice);
            if (blocked.HasValue)
            {
                return OperationResult<InvoiceItemConfirmationDTO>.Fail(blocked.Value);
            }

            _mapper.Map(item, existing);
            existing.TotalAmount = existing.UnitPrice * existing.Quantity;

            RecalculateInvoice(invoice);
            _context.SaveChanges();

            return OperationResult<InvoiceItemConfirmationDTO>.Ok(BuildConfirmation(existing, invoice));
        }

        public OperationResult<bool> DeleteItem(Guid invoiceItemId)
        {
            var item = _context.InvoiceItems.FirstOrDefault(s => s.InvoiceItemId == invoiceItemId);

            if (item is null)
            {
                return OperationResult<bool>.Fail(OperationOutcome.NotFound);
            }

            var invoice = LoadInvoice(item.InvoiceId);

            if (invoice is null)
            {
                return OperationResult<bool>.Fail(OperationOutcome.NotFound);
            }

            var blocked = CheckInvoiceEditable(invoice);
            if (blocked.HasValue)
            {
                return OperationResult<bool>.Fail(blocked.Value);
            }

            invoice.Items.Remove(item);
            _context.InvoiceItems.Remove(item);
            RecalculateInvoice(invoice);
            _context.SaveChanges();

            return OperationResult<bool>.Ok(true);
        }

        private Invoice? LoadInvoice(Guid invoiceId)
        {
            return _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);
        }

        //stavke se ne menjaju na placenoj ni na storniranoj fakturi
        private static OperationOutcome? CheckInvoiceEditable(Invoice invoice)
        {
            if (invoice.Status == InvoiceStatus.Paid)
            {
                return OperationOutcome.InvoiceIsPaid;
            }

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                return OperationOutcome.InvoiceIsCancelled;
            }

            return null;
        }

        //ukupan iznos je uvek zbir stavki, a status prati koliko je do sada uplaceno
        private static void RecalculateInvoice(Invoice invoice)
        {
            invoice.TotalAmount = invoice.Items.Sum(s => s.TotalAmount);

            var paid = invoice.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            if (invoice.TotalAmount > 0 && paid >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
        }

        private InvoiceItemConfirmationDTO BuildConfirmation(InvoiceItem item, Invoice invoice)
        {
            var confirmation = _mapper.Map<InvoiceItemConfirmationDTO>(item);
            confirmation.InvoiceTotalAmount = invoice.TotalAmount;
            return confirmation;
        }
    }
}
