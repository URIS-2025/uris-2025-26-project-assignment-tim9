using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Data
{
    public interface IPaymentRepository
    {
        IEnumerable<PaymentDTO> GetPayments(Guid? invoiceId = null, Guid? paidByUserId = null);
        PaymentDTO? GetPaymentById(Guid paymentId);
        Task<OperationResult<PaymentConfirmationDTO>> CreatePaymentAsync(PaymentCreationDTO payment, Guid paidByUserId);
        Task<OperationResult<PaymentConfirmationDTO>> UpdatePaymentAsync(Guid paymentId, PaymentUpdateDTO payment);
        OperationResult<bool> DeletePayment(Guid paymentId);
    }
}
