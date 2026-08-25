using PaymentService.Data;
using PaymentService.Models.DTO.PaymentDTOs;
using PaymentService.Models.DTO.User;
using PaymentService.Models.Enums;
using Moq;

namespace PaymentService.Tests
{
    public class PaymentRepositoryTests
    {
        [Fact]
        public async Task CreatePayment_WhenInvoiceDoesNotExist_ReturnsNotFound()
        {
            var fx = new TestFixture();

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = Guid.NewGuid(), Amount = 100m },
                Guid.NewGuid(), false);

            Assert.Equal(OperationOutcome.NotFound, result.Outcome);
        }

        [Fact]
        public async Task CreatePayment_WhenAmountExceedsDebt_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 1500m },
                Guid.NewGuid(), false);

            Assert.Equal(OperationOutcome.AmountExceedsRemainingDebt, result.Outcome);
            Assert.Empty(fx.Context.Payments);
        }

        [Fact]
        public async Task CreatePayment_WhenAmountCoversInvoice_MarksInvoiceAsPaid()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 1000m },
                Guid.NewGuid(), false);

            Assert.True(result.IsSuccess);
            Assert.Equal(InvoiceStatus.Paid, invoice.Status);
            Assert.Equal(PaymentStatus.Completed, result.Value!.Status);
        }

        [Fact]
        public async Task CreatePayment_WithPartialAmount_LeavesInvoiceUnpaid()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 400m },
                Guid.NewGuid(), false);

            Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        }

        [Fact]
        public async Task CreatePayment_AfterPartialPayment_AllowsOnlyRemainingAmount()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();
            var payer = Guid.NewGuid();

            await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 400m }, payer, false);

            var tooMuch = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 700m }, payer, false);

            Assert.Equal(OperationOutcome.AmountExceedsRemainingDebt, tooMuch.Outcome);

            var exact = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 600m }, payer, false);

            Assert.True(exact.IsSuccess);
            Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        }

        [Fact]
        public async Task CreatePayment_WhenInvoiceAlreadyPaid_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice(InvoiceStatus.Paid);

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 10m },
                Guid.NewGuid(), false);

            Assert.Equal(OperationOutcome.InvoiceIsPaid, result.Outcome);
        }

        [Fact]
        public async Task CreatePayment_WhenInvoiceCancelled_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice(InvoiceStatus.Cancelled);

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 10m },
                Guid.NewGuid(), false);

            Assert.Equal(OperationOutcome.InvoiceIsCancelled, result.Outcome);
        }

        [Fact]
        public async Task CreatePayment_FillsDataFromOtherServices()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 100m },
                Guid.NewGuid(), false);

            Assert.Equal(TestFixture.KnownUsername, result.Value!.PaidByUsername);
            Assert.Equal(TestFixture.KnownProjectName, result.Value.ProjectName);
        }

        [Fact]
        public async Task CreatePayment_WhenUserServiceUnavailable_UsesFallbackName()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            //servis nedostupan vraca null, uplata i dalje mora da prodje
            fx.UserService
                .Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync((UserInfoDTO?)null);

            var result = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 100m },
                Guid.NewGuid(), false);

            Assert.True(result.IsSuccess);
            Assert.Equal("Nepoznat korisnik", result.Value!.PaidByUsername);
        }

        [Fact]
        public async Task UpdatePayment_DoesNotCountItsOwnAmountAsDebt()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var created = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 500m },
                Guid.NewGuid(), false);

            //povecanje sa 500 na 800 je ispravno jer se stara uplata ne racuna dva puta
            var updated = await fx.Payments.UpdatePaymentAsync(
                created.Value!.PaymentId,
                new PaymentUpdateDTO { Amount = 800m });

            Assert.True(updated.IsSuccess);
            Assert.Equal(800m, updated.Value!.Amount);
        }

        [Fact]
        public async Task DeletePayment_ReturnsInvoiceToUnpaid()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var created = await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = invoice.InvoiceId, Amount = 1000m },
                Guid.NewGuid(), false);

            Assert.Equal(InvoiceStatus.Paid, invoice.Status);

            var deleted = fx.Payments.DeletePayment(created.Value!.PaymentId);

            Assert.True(deleted.IsSuccess);
            Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        }

        [Fact]
        public async Task GetPayments_FiltersByInvoice()
        {
            var fx = new TestFixture();
            var first = fx.SeedInvoice();
            var second = fx.SeedInvoice();

            await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = first.InvoiceId, Amount = 100m }, Guid.NewGuid(), false);
            await fx.Payments.CreatePaymentAsync(
                new PaymentCreationDTO { InvoiceId = second.InvoiceId, Amount = 200m }, Guid.NewGuid(), false);

            var payments = fx.Payments.GetPayments(invoiceId: first.InvoiceId).ToList();

            Assert.Single(payments);
            Assert.Equal(100m, payments[0].Amount);
        }
    }
}
