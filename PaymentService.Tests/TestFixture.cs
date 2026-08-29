using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentService.Context;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Models.DTO.Project;
using PaymentService.Models.DTO.User;
using PaymentService.Models.Enums;
using PaymentService.Profiles;
using PaymentService.ServiceCalls.Project;
using PaymentService.ServiceCalls.User;

namespace PaymentService.Tests
{
    //zajednicka priprema za testove repozitorijuma
    public sealed class TestFixture
    {
        public PaymentContext Context { get; }
        public IMapper Mapper { get; }
        public Mock<IUserService> UserService { get; }
        public Mock<IProjectService> ProjectService { get; }

        public InvoiceRepository Invoices { get; }
        public InvoiceItemRepository Items { get; }
        public PaymentRepository Payments { get; }

        public const string KnownUsername = "test.korisnik";
        public const string KnownProjectName = "Test projekat";

        public TestFixture()
        {
            //svaki test dobija svoju bazu u memoriji
            var options = new DbContextOptionsBuilder<PaymentContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new PaymentContext(options, new ConfigurationBuilder().Build());
            ClearSeedData();

            Mapper = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<InvoiceProfile>();
                    cfg.AddProfile<InvoiceItemProfile>();
                    cfg.AddProfile<PaymentProfile>();
                },
                NullLoggerFactory.Instance).CreateMapper();

            UserService = new Mock<IUserService>();
            UserService
                .Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new UserInfoDTO { Username = KnownUsername, Email = "test@example.com" });

            ProjectService = new Mock<IProjectService>();
            ProjectService
                .Setup(s => s.GetProjectInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new ProjectInfoDTO { Name = KnownProjectName, Budget = 100000 });

            //podrazumevano je pozivalac clan projekta; testovi koji proveravaju
            //suprotno ovo pregaze svojim Setup-om
            ProjectService
                .Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.Member));

            //podrazumevano korisnik nije ni na jednom projektu; testovi filtriranja
            //ovo pregaze svojim Setup-om, a testovi koji ne testiraju vidljivost
            //citaju kao admin
            ProjectService
                .Setup(s => s.GetProjectIdsForUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Array.Empty<Guid>());

            Invoices = new InvoiceRepository(Context, Mapper, UserService.Object, ProjectService.Object);
            Items = new InvoiceItemRepository(Context, Mapper);
            Payments = new PaymentRepository(Context, Mapper, UserService.Object, ProjectService.Object);
        }

        //kontekst nosi pocetne podatke iz HasData, testovi krecu od prazne baze
        private void ClearSeedData()
        {
            Context.Payments.RemoveRange(Context.Payments);
            Context.InvoiceItems.RemoveRange(Context.InvoiceItems);
            Context.Invoices.RemoveRange(Context.Invoices);
            Context.SaveChanges();
        }

        //faktura na 1000 sa dve stavke
        public Invoice SeedInvoice(InvoiceStatus status = InvoiceStatus.Unpaid, decimal unitPrice = 100m, int quantity = 10)
        {
            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                IssueDate = new DateTime(2026, 8, 1),
                Status = status,
                IssuedByUserId = Guid.NewGuid(),
                TotalAmount = unitPrice * quantity
            };

            invoice.Items.Add(new InvoiceItem
            {
                InvoiceItemId = Guid.NewGuid(),
                InvoiceId = invoice.InvoiceId,
                Description = "Stavka",
                UnitPrice = unitPrice,
                Quantity = quantity,
                TotalAmount = unitPrice * quantity
            });

            Context.Invoices.Add(invoice);
            Context.SaveChanges();

            return invoice;
        }
    }
}
