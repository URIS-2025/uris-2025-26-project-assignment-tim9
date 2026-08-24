using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentService.Models;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.Enums;
using PaymentService.Profiles;

namespace PaymentService.Tests
{
    public class ProfileTests
    {
        private static MapperConfiguration BuildConfiguration()
        {
            return new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<InvoiceProfile>();
                    cfg.AddProfile<InvoiceItemProfile>();
                    cfg.AddProfile<PaymentProfile>();
                },
                NullLoggerFactory.Instance);
        }

        [Fact]
        public void Configuration_IsValid()
        {
            //puca ako neko polje odredista nije ni mapirano ni eksplicitno ignorisano
            BuildConfiguration().AssertConfigurationIsValid();
        }

        [Fact]
        public void UpdateMapping_DoesNotOverwriteWithNulls()
        {
            var mapper = BuildConfiguration().CreateMapper();

            var item = new InvoiceItem
            {
                InvoiceItemId = Guid.NewGuid(),
                Description = "Originalni opis",
                UnitPrice = 100m,
                Quantity = 5
            };

            //u zahtevu je poslata samo kolicina
            mapper.Map(new InvoiceItemUpdateDTO { Quantity = 8 }, item);

            Assert.Equal(8, item.Quantity);
            Assert.Equal("Originalni opis", item.Description);
            Assert.Equal(100m, item.UnitPrice);
        }

        [Fact]
        public void CreationMapping_IgnoresFieldsSetByService()
        {
            var mapper = BuildConfiguration().CreateMapper();

            var item = mapper.Map<InvoiceItem>(new InvoiceItemCreationDTO
            {
                Description = "Nova stavka",
                UnitPrice = 20m,
                Quantity = 3
            });

            //id, veza sa fakturom i iznos ne dolaze iz zahteva
            Assert.Equal(Guid.Empty, item.InvoiceItemId);
            Assert.Equal(Guid.Empty, item.InvoiceId);
            Assert.Equal(0m, item.TotalAmount);
        }

        [Fact]
        public void InvoiceConfirmation_CountsItems()
        {
            var mapper = BuildConfiguration().CreateMapper();

            var invoice = new Invoice { InvoiceId = Guid.NewGuid(), Status = InvoiceStatus.Unpaid };
            invoice.Items.Add(new InvoiceItem());
            invoice.Items.Add(new InvoiceItem());

            var confirmation = mapper.Map<PaymentService.Models.DTO.InvoiceDTOs.InvoiceConfirmationDTO>(invoice);

            Assert.Equal(2, confirmation.ItemCount);
        }
    }
}
