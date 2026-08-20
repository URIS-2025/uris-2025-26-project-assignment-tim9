using System.ComponentModel.DataAnnotations;
using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Tests
{
    public class ValidationTests
    {
        //pokrece iste provere koje ASP.NET radi nad telom zahteva
        private static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void InvoiceItem_WithZeroQuantity_IsInvalid()
        {
            var results = Validate(new InvoiceItemCreationDTO
            {
                Description = "Stavka",
                UnitPrice = 10m,
                Quantity = 0
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InvoiceItemCreationDTO.Quantity)));
        }

        [Fact]
        public void InvoiceItem_WithZeroPrice_IsInvalid()
        {
            var results = Validate(new InvoiceItemCreationDTO
            {
                Description = "Stavka",
                UnitPrice = 0m,
                Quantity = 1
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InvoiceItemCreationDTO.UnitPrice)));
        }

        [Fact]
        public void InvoiceItem_WithTooShortDescription_IsInvalid()
        {
            var results = Validate(new InvoiceItemCreationDTO
            {
                Description = "a",
                UnitPrice = 10m,
                Quantity = 1
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InvoiceItemCreationDTO.Description)));
        }

        [Fact]
        public void InvoiceItem_WithValidData_PassesValidation()
        {
            var results = Validate(new InvoiceItemCreationDTO
            {
                Description = "Ispravna stavka",
                UnitPrice = 10m,
                Quantity = 1
            });

            Assert.Empty(results);
        }

        [Fact]
        public void Invoice_WithFutureIssueDate_IsInvalid()
        {
            var results = Validate(new InvoiceCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                IssueDate = DateTime.Now.AddDays(3),
                Items = new List<InvoiceItemCreationDTO>
                {
                    new InvoiceItemCreationDTO { Description = "Stavka", UnitPrice = 10m, Quantity = 1 }
                }
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InvoiceCreationDTO.IssueDate)));
        }

        [Fact]
        public void Invoice_WithoutItems_IsInvalid()
        {
            var results = Validate(new InvoiceCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                IssueDate = new DateTime(2026, 8, 1),
                Items = new List<InvoiceItemCreationDTO>()
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InvoiceCreationDTO.Items)));
        }

        [Fact]
        public void Invoice_WithEmptyProjectId_IsInvalid()
        {
            var results = Validate(new InvoiceCreationDTO
            {
                ProjectId = Guid.Empty,
                IssueDate = new DateTime(2026, 8, 1),
                Items = new List<InvoiceItemCreationDTO>
                {
                    new InvoiceItemCreationDTO { Description = "Stavka", UnitPrice = 10m, Quantity = 1 }
                }
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(InvoiceCreationDTO.ProjectId)));
        }

        [Fact]
        public void Payment_WithEmptyInvoiceId_IsInvalid()
        {
            var results = Validate(new PaymentCreationDTO
            {
                InvoiceId = Guid.Empty,
                Amount = 100m
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(PaymentCreationDTO.InvoiceId)));
        }

        [Fact]
        public void Payment_WithNegativeAmount_IsInvalid()
        {
            var results = Validate(new PaymentCreationDTO
            {
                InvoiceId = Guid.NewGuid(),
                Amount = -5m
            });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(PaymentCreationDTO.Amount)));
        }
    }
}
