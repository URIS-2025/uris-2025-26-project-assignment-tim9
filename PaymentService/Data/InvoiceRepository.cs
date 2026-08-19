using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PaymentService.Context;
using PaymentService.Models;
using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Data
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly PaymentContext _context;
        private readonly IMapper _mapper;

        public InvoiceRepository(PaymentContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<InvoiceDTO> GetInvoices(Guid? projectId = null, InvoiceStatus? status = null)
        {
            var query = _context.Invoices.Include(i => i.Items).AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(i => i.ProjectId == projectId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var invoices = query.OrderByDescending(i => i.IssueDate).ToList();
            return _mapper.Map<List<InvoiceDTO>>(invoices);
        }

        public InvoiceDTO? GetInvoiceById(Guid invoiceId)
        {
            var invoice = _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);

            return invoice is null ? null : _mapper.Map<InvoiceDTO>(invoice);
        }

        public InvoiceConfirmationDTO CreateInvoice(InvoiceCreationDTO invoice, Guid issuedByUserId)
        {
            var newInvoice = _mapper.Map<Invoice>(invoice);
            newInvoice.InvoiceId = Guid.NewGuid();
            newInvoice.IssuedByUserId = issuedByUserId;
            newInvoice.Status = InvoiceStatus.Unpaid;

            //svaka stavka dobija id i izracunat iznos, ukupan iznos je njihov zbir
            foreach (var item in newInvoice.Items)
            {
                item.InvoiceItemId = Guid.NewGuid();
                item.InvoiceId = newInvoice.InvoiceId;
                item.TotalAmount = item.UnitPrice * item.Quantity;
            }

            newInvoice.TotalAmount = newInvoice.Items.Sum(i => i.TotalAmount);

            _context.Invoices.Add(newInvoice);
            _context.SaveChanges();

            return _mapper.Map<InvoiceConfirmationDTO>(newInvoice);
        }

        public OperationResult<InvoiceConfirmationDTO> UpdateInvoice(Guid invoiceId, InvoiceUpdateDTO invoice)
        {
            var existing = _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);

            if (existing is null)
            {
                return OperationResult<InvoiceConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            //placena faktura se ne dira
            if (existing.Status == InvoiceStatus.Paid)
            {
                return OperationResult<InvoiceConfirmationDTO>.Fail(OperationOutcome.InvoiceIsPaid);
            }

            _mapper.Map(invoice, existing);
            _context.SaveChanges();

            return OperationResult<InvoiceConfirmationDTO>.Ok(_mapper.Map<InvoiceConfirmationDTO>(existing));
        }

        public OperationResult<bool> DeleteInvoice(Guid invoiceId)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);

            if (invoice is null)
            {
                return OperationResult<bool>.Fail(OperationOutcome.NotFound);
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                return OperationResult<bool>.Fail(OperationOutcome.InvoiceIsPaid);
            }

            //stavke i uplate odlaze sa fakturom, kaskadno brisanje je podeseno u kontekstu
            _context.Invoices.Remove(invoice);
            _context.SaveChanges();

            return OperationResult<bool>.Ok(true);
        }
    }
}
