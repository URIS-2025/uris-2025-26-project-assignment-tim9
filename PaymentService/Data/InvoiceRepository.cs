using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PaymentService.Context;
using PaymentService.Models;
using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.Enums;
using PaymentService.ServiceCalls.Project;
using PaymentService.ServiceCalls.User;

namespace PaymentService.Data
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly PaymentContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        public InvoiceRepository(
            PaymentContext context,
            IMapper mapper,
            IUserService userService,
            IProjectService projectService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _projectService = projectService;
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

        public async Task<InvoiceConfirmationDTO> CreateInvoiceAsync(InvoiceCreationDTO invoice, Guid issuedByUserId)
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

            return await BuildConfirmationAsync(newInvoice);
        }

        public async Task<OperationResult<InvoiceConfirmationDTO>> UpdateInvoiceAsync(Guid invoiceId, InvoiceUpdateDTO invoice)
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

            return OperationResult<InvoiceConfirmationDTO>.Ok(await BuildConfirmationAsync(existing));
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

        //naziv projekta i korisnicko ime izdavaoca dolaze iz drugih servisa
        private async Task<InvoiceConfirmationDTO> BuildConfirmationAsync(Invoice invoice)
        {
            var confirmation = _mapper.Map<InvoiceConfirmationDTO>(invoice);

            var project = await _projectService.GetProjectInfoAsync(invoice.ProjectId);
            var issuer = await _userService.GetUserInfoAsync(invoice.IssuedByUserId);

            confirmation.ProjectName = project?.Name ?? "Nepoznat projekat";
            confirmation.IssuedByUsername = issuer?.Username ?? "Nepoznat korisnik";

            return confirmation;
        }
    }
}
