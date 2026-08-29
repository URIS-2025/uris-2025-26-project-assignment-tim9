using Moq;
using PaymentService.Data;
using PaymentService.Models.DTO.InvoiceDTOs;
using PaymentService.Models.DTO.InvoiceItemDTOs;
using PaymentService.Models.Enums;
using PaymentService.ServiceCalls.Project;

namespace PaymentService.Tests
{
    public class InvoiceRepositoryTests
    {
        private static InvoiceCreationDTO NewInvoiceDto(Guid projectId) => new InvoiceCreationDTO
        {
            ProjectId = projectId,
            IssueDate = new DateTime(2026, 8, 1),
            Items = new List<InvoiceItemCreationDTO>
            {
                new InvoiceItemCreationDTO { Description = "Analiza", UnitPrice = 50m, Quantity = 10 },
                new InvoiceItemCreationDTO { Description = "Razvoj", UnitPrice = 100m, Quantity = 10 }
            }
        };

        [Fact]
        public async Task CreateInvoice_CalculatesTotalFromItems()
        {
            var fx = new TestFixture();

            var confirmation = (await fx.Invoices.CreateInvoiceAsync(NewInvoiceDto(Guid.NewGuid()), Guid.NewGuid(), false)).Value!;

            //10*50 + 10*100
            Assert.Equal(1500m, confirmation.TotalAmount);
            Assert.Equal(2, confirmation.ItemCount);
        }

        [Fact]
        public async Task CreateInvoice_SetsStatusToUnpaidAndFillsIssuer()
        {
            var fx = new TestFixture();
            var issuer = Guid.NewGuid();

            var confirmation = (await fx.Invoices.CreateInvoiceAsync(NewInvoiceDto(Guid.NewGuid()), issuer, false)).Value!;

            Assert.Equal(InvoiceStatus.Unpaid, confirmation.Status);
            Assert.Equal(TestFixture.KnownUsername, confirmation.IssuedByUsername);
            Assert.Equal(TestFixture.KnownProjectName, confirmation.ProjectName);
        }

        [Fact]
        public async Task CreateInvoice_CalculatesAmountForEachItem()
        {
            var fx = new TestFixture();

            var confirmation = (await fx.Invoices.CreateInvoiceAsync(NewInvoiceDto(Guid.NewGuid()), Guid.NewGuid(), false)).Value!;
            var items = fx.Items.GetItemsByInvoiceId(confirmation.InvoiceId).ToList();

            Assert.Equal(500m, items.Single(i => i.Description == "Analiza").TotalAmount);
            Assert.Equal(1000m, items.Single(i => i.Description == "Razvoj").TotalAmount);
        }

        [Fact]
        public async Task UpdateInvoice_WhenInvoiceIsPaid_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice(InvoiceStatus.Paid);

            var result = await fx.Invoices.UpdateInvoiceAsync(
                invoice.InvoiceId,
                new InvoiceUpdateDTO { Status = InvoiceStatus.Cancelled });

            Assert.Equal(OperationOutcome.InvoiceIsPaid, result.Outcome);
            Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        }

        [Fact]
        public async Task UpdateInvoice_WithPartialData_KeepsOtherFields()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();
            var originalProject = invoice.ProjectId;
            var originalDate = invoice.IssueDate;

            //salje se samo status, ostala polja moraju da ostanu netaknuta
            var result = await fx.Invoices.UpdateInvoiceAsync(
                invoice.InvoiceId,
                new InvoiceUpdateDTO { Status = InvoiceStatus.Cancelled });

            Assert.True(result.IsSuccess);
            Assert.Equal(originalProject, invoice.ProjectId);
            Assert.Equal(originalDate, invoice.IssueDate);
            Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        }

        [Fact]
        public async Task UpdateInvoice_WhenMissing_ReturnsNotFound()
        {
            var fx = new TestFixture();

            var result = await fx.Invoices.UpdateInvoiceAsync(Guid.NewGuid(), new InvoiceUpdateDTO());

            Assert.Equal(OperationOutcome.NotFound, result.Outcome);
        }

        [Fact]
        public void DeleteInvoice_WhenPaid_IsRejected()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice(InvoiceStatus.Paid);

            var result = fx.Invoices.DeleteInvoice(invoice.InvoiceId);

            Assert.Equal(OperationOutcome.InvoiceIsPaid, result.Outcome);
            Assert.Single(fx.Context.Invoices);
        }

        [Fact]
        public void DeleteInvoice_WhenUnpaid_RemovesIt()
        {
            var fx = new TestFixture();
            var invoice = fx.SeedInvoice();

            var result = fx.Invoices.DeleteInvoice(invoice.InvoiceId);

            Assert.True(result.IsSuccess);
            Assert.Empty(fx.Context.Invoices);
        }

        [Fact]
        public async Task GetInvoices_FiltersByProjectAndStatus()
        {
            var fx = new TestFixture();
            var paid = fx.SeedInvoice(InvoiceStatus.Paid);
            fx.SeedInvoice();

            var byProject = (await fx.Invoices.GetInvoicesAsync(Guid.NewGuid(), isAdmin: true, projectId: paid.ProjectId)).ToList();
            var byStatus = (await fx.Invoices.GetInvoicesAsync(Guid.NewGuid(), isAdmin: true, status: InvoiceStatus.Unpaid)).ToList();

            Assert.Single(byProject);
            Assert.Single(byStatus);
            Assert.Equal(InvoiceStatus.Unpaid, byStatus[0].Status);
        }
        [Fact]
        public async Task CreateInvoice_WhenIssuerIsNotProjectMember_Fails()
        {
            var fx = new TestFixture();
            fx.ProjectService
                .Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            var result = await fx.Invoices.CreateInvoiceAsync(NewInvoiceDto(Guid.NewGuid()), Guid.NewGuid(), false);

            Assert.False(result.IsSuccess);
            Assert.Equal(OperationOutcome.NotProjectMember, result.Outcome);
        }

        [Fact]
        public async Task CreateInvoice_WhenProjectServiceIsUnavailable_StillSucceeds()
        {
            var fx = new TestFixture();
            fx.ProjectService
                .Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable));

            var result = await fx.Invoices.CreateInvoiceAsync(NewInvoiceDto(Guid.NewGuid()), Guid.NewGuid(), false);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateInvoice_AsAdmin_SkipsMembershipCheck()
        {
            var fx = new TestFixture();
            fx.ProjectService
                .Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            var result = await fx.Invoices.CreateInvoiceAsync(NewInvoiceDto(Guid.NewGuid()), Guid.NewGuid(), true);

            Assert.True(result.IsSuccess);
            fx.ProjectService.Verify(
                s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetInvoices_HidesInvoicesFromProjectsTheCallerIsNotOn()
        {
            var fx = new TestFixture();
            var mine = fx.SeedInvoice();
            fx.SeedInvoice();

            fx.ProjectService
                .Setup(s => s.GetProjectIdsForUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new[] { mine.ProjectId });

            var visible = (await fx.Invoices.GetInvoicesAsync(Guid.NewGuid(), isAdmin: false)).ToList();

            Assert.Single(visible);
            Assert.Equal(mine.InvoiceId, visible[0].InvoiceId);
        }

        [Fact]
        public async Task GetInvoices_AlwaysShowsInvoicesTheCallerIssued()
        {
            var fx = new TestFixture();
            var issuer = Guid.NewGuid();
            var mine = fx.SeedInvoice();
            mine.IssuedByUserId = issuer;
            fx.Context.SaveChanges();

            fx.SeedInvoice();

            //Project servis ne odgovara, ali svoje fakture korisnik i dalje vidi
            fx.ProjectService
                .Setup(s => s.GetProjectIdsForUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IReadOnlyCollection<Guid>?)null);

            var visible = (await fx.Invoices.GetInvoicesAsync(issuer, isAdmin: false)).ToList();

            Assert.Single(visible);
            Assert.Equal(mine.InvoiceId, visible[0].InvoiceId);
        }

        [Fact]
        public async Task GetInvoices_AsAdmin_ShowsEverything()
        {
            var fx = new TestFixture();
            fx.SeedInvoice();
            fx.SeedInvoice();

            var visible = (await fx.Invoices.GetInvoicesAsync(Guid.NewGuid(), isAdmin: true)).ToList();

            Assert.Equal(2, visible.Count);
            fx.ProjectService.Verify(s => s.GetProjectIdsForUserAsync(It.IsAny<Guid>()), Times.Never);
        }

    }
}
