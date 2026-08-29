using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Data
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<PaymentDTO>> GetPaymentsAsync(Guid callerId, bool isAdmin, Guid? invoiceId = null, Guid? paidByUserId = null);
        PaymentDTO? GetPaymentById(Guid paymentId);
        Task<OperationResult<PaymentConfirmationDTO>> CreatePaymentAsync(PaymentCreationDTO payment, Guid paidByUserId, bool isAdmin);
        Task<OperationResult<PaymentConfirmationDTO>> UpdatePaymentAsync(Guid paymentId, PaymentUpdateDTO payment);
        OperationResult<bool> DeletePayment(Guid paymentId);
    }
}
