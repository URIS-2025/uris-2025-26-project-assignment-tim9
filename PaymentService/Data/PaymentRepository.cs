using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PaymentService.Context;
using PaymentService.Models;
using PaymentService.Models.DTO.PaymentDTOs;
using PaymentService.Models.Enums;
using PaymentService.ServiceCalls.Project;
using PaymentService.ServiceCalls.User;

namespace PaymentService.Data
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        public PaymentRepository(
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

        public IEnumerable<PaymentDTO> GetPayments(Guid? invoiceId = null, Guid? paidByUserId = null)
        {
            var query = _context.Payments.AsQueryable();

            if (invoiceId.HasValue)
            {
                query = query.Where(p => p.InvoiceId == invoiceId.Value);
            }

            if (paidByUserId.HasValue)
            {
                query = query.Where(p => p.PaidByUserId == paidByUserId.Value);
            }

            var payments = query.OrderByDescending(p => p.Date).ToList();
            return _mapper.Map<List<PaymentDTO>>(payments);
        }

        public PaymentDTO? GetPaymentById(Guid paymentId)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
            return payment is null ? null : _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<OperationResult<PaymentConfirmationDTO>> CreatePaymentAsync(PaymentCreationDTO payment, Guid paidByUserId)
        {
            var invoice = LoadInvoice(payment.InvoiceId);

            if (invoice is null)
            {
                return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.InvoiceIsCancelled);
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.InvoiceIsPaid);
            }

            //ne moze da se uplati vise nego sto je ostalo da se plati
            if (payment.Amount > RemainingDebt(invoice))
            {
                return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.AmountExceedsRemainingDebt);
            }

            var newPayment = _mapper.Map<Payment>(payment);
            newPayment.PaymentId = Guid.NewGuid();
            newPayment.PaidByUserId = paidByUserId;
            newPayment.Date = DateTime.Now;
            newPayment.Status = PaymentStatus.Completed;

            invoice.Payments.Add(newPayment);
            RefreshInvoiceStatus(invoice);
            _context.SaveChanges();

            return OperationResult<PaymentConfirmationDTO>.Ok(await BuildConfirmationAsync(newPayment, invoice));
        }

        public async Task<OperationResult<PaymentConfirmationDTO>> UpdatePaymentAsync(Guid paymentId, PaymentUpdateDTO payment)
        {
            var existing = _context.Payments.FirstOrDefault(p => p.PaymentId == paymentId);

            if (existing is null)
            {
                return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            var invoice = LoadInvoice(existing.InvoiceId);

            if (invoice is null)
            {
                return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.NotFound);
            }

            //izmena iznosa ne sme da probije ostatak duga, bez racunanja same ove uplate
            if (payment.Amount.HasValue)
            {
                var debtWithoutThisPayment = RemainingDebt(invoice, ignorePaymentId: paymentId);

                if (payment.Amount.Value > debtWithoutThisPayment)
                {
                    return OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.AmountExceedsRemainingDebt);
                }
            }

            _mapper.Map(payment, existing);
            RefreshInvoiceStatus(invoice);
            _context.SaveChanges();

            return OperationResult<PaymentConfirmationDTO>.Ok(await BuildConfirmationAsync(existing, invoice));
        }

        public OperationResult<bool> DeletePayment(Guid paymentId)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.PaymentId == paymentId);

            if (payment is null)
            {
                return OperationResult<bool>.Fail(OperationOutcome.NotFound);
            }

            var invoice = LoadInvoice(payment.InvoiceId);

            invoice?.Payments.Remove(payment);
            _context.Payments.Remove(payment);

            //brisanjem uplate faktura moze da se vrati u neplaceno stanje
            if (invoice is not null)
            {
                RefreshInvoiceStatus(invoice);
            }

            _context.SaveChanges();

            return OperationResult<bool>.Ok(true);
        }

        private Invoice? LoadInvoice(Guid invoiceId)
        {
            return _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);
        }

        private static decimal PaidSoFar(Invoice invoice, Guid? ignorePaymentId = null)
        {
            return invoice.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Where(p => ignorePaymentId == null || p.PaymentId != ignorePaymentId.Value)
                .Sum(p => p.Amount);
        }

        private static decimal RemainingDebt(Invoice invoice, Guid? ignorePaymentId = null)
        {
            return invoice.TotalAmount - PaidSoFar(invoice, ignorePaymentId);
        }

        //faktura je placena kada zbir uspesnih uplata dostigne njen ukupan iznos
        private static void RefreshInvoiceStatus(Invoice invoice)
        {
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                return;
            }

            invoice.Status = invoice.TotalAmount > 0 && PaidSoFar(invoice) >= invoice.TotalAmount
                ? InvoiceStatus.Paid
                : InvoiceStatus.Unpaid;
        }

        //ime platioca i naziv projekta dolaze iz drugih servisa
        private async Task<PaymentConfirmationDTO> BuildConfirmationAsync(Payment payment, Invoice invoice)
        {
            var confirmation = _mapper.Map<PaymentConfirmationDTO>(payment);

            var payer = await _userService.GetUserInfoAsync(payment.PaidByUserId);
            var project = await _projectService.GetProjectInfoAsync(invoice.ProjectId);

            confirmation.PaidByUsername = payer?.Username ?? "Nepoznat korisnik";
            confirmation.ProjectName = project?.Name ?? "Nepoznat projekat";

            return confirmation;
        }
    }
}
