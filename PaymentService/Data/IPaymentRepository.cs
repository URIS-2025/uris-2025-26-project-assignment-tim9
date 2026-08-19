using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Data
{
    public interface IPaymentRepository
    {
        IEnumerable<PaymentDTO> GetPayments(Guid? invoiceId = null, Guid? paidByUserId = null);
        PaymentDTO? GetPaymentById(Guid paymentId);
        OperationResult<PaymentConfirmationDTO> CreatePayment(PaymentCreationDTO payment, Guid paidByUserId);
        OperationResult<PaymentConfirmationDTO> UpdatePayment(Guid paymentId, PaymentUpdateDTO payment);
        OperationResult<bool> DeletePayment(Guid paymentId);
    }
}
