using PaymentService.Data;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Tests
{
    public class InvoiceItemRepositoryTests
    {
        [Fact]
        public void AddItem_RecalculatesInvoiceTotal()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var result = fx.Items.AddItem(invoice.InvoiceId, new InvoiceItemCreationDTO
            {
                Description = "Dodatno",
                UnitPrice = 25m,
                Quantity = 4
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(100m, result.Value!.TotalAmount);
            Assert.Equal(1100m, result.Value.InvoiceTotalAmount);
            Assert.Equal(1100m, invoice.TotalAmount);
        }

        [Fact]
        public void AddItem_WhenInvoiceMissing_ReturnsNotFound()
        {
            var fx = new TestFixture();

            var result = fx.Items.AddItem(Guid.NewGuid(), new InvoiceItemCreationDTO
            {
                Description = "Dodatno",
                UnitPrice = 25m,
                Quantity = 4
            });

            Assert.Equal(OperationOutcome.NotFound, result.Outcome);
        }

        [Fact]
        public void AddItem_WhenInvoicePaid_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice(InvoiceStatus.Paid);

            var result = fx.Items.AddItem(invoice.InvoiceId, new InvoiceItemCreationDTO
            {
                Description = "Dodatno",
                UnitPrice = 25m,
                Quantity = 4
            });

            Assert.Equal(OperationOutcome.InvoiceIsPaid, result.Outcome);
        }

        [Fact]
        public void AddItem_WhenInvoiceCancelled_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice(InvoiceStatus.Cancelled);

            var result = fx.Items.AddItem(invoice.InvoiceId, new InvoiceItemCreationDTO
            {
                Description = "Dodatno",
                UnitPrice = 25m,
                Quantity = 4
            });

            Assert.Equal(OperationOutcome.InvoiceIsCancelled, result.Outcome);
        }

        [Fact]
        public void UpdateItem_RecalculatesItemAndInvoice()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();
            var item = invoice.Items.First();

            //sa 10 na 12 komada po 100
            var result = fx.Items.UpdateItem(item.InvoiceItemId, new InvoiceItemUpdateDTO { Quantity = 12 });

            Assert.True(result.IsSuccess);
            Assert.Equal(1200m, result.Value!.TotalAmount);
            Assert.Equal(1200m, invoice.TotalAmount);
        }

        [Fact]
        public void UpdateItem_WithOnlyQuantity_KeepsDescriptionAndPrice()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();
            var item = invoice.Items.First();

            fx.Items.UpdateItem(item.InvoiceItemId, new InvoiceItemUpdateDTO { Quantity = 3 });

            Assert.Equal("Stavka", item.Description);
            Assert.Equal(100m, item.UnitPrice);
        }

        [Fact]
        public void DeleteItem_RecalculatesInvoiceTotal()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            fx.Items.AddItem(invoice.InvoiceId, new InvoiceItemCreationDTO
            {
                Description = "Druga stavka",
                UnitPrice = 50m,
                Quantity = 2
            });

            Assert.Equal(1100m, invoice.TotalAmount);

            var toRemove = invoice.Items.First(i => i.Description == "Druga stavka");
            var result = fx.Items.DeleteItem(toRemove.InvoiceItemId);

            Assert.True(result.IsSuccess);
            Assert.Equal(1000m, invoice.TotalAmount);
        }

        [Fact]
        public void DeleteItem_WhenMissing_ReturnsNotFound()
        {
            var fx = new TestFixture();

            var result = fx.Items.DeleteItem(Guid.NewGuid());

            Assert.Equal(OperationOutcome.NotFound, result.Outcome);
        }
    }
}
