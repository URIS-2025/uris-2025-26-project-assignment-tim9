using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using PaymentService.Models.Enums;

namespace PaymentService.Context
{
    public class PaymentContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public PaymentContext(DbContextOptions<PaymentContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        //tabele
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;

        //konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("PaymentDB");

                //verzija je zakucana namerno. ServerVersion.AutoDetect otvara novu
                //konekciju ka bazi pri svakom zahtevu, jer se OnConfiguring poziva
                //za svaku instancu konteksta - to trosi konekcije i usporava servis.
                optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0))
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //novcane kolone - decimal(18,2), inace ih MySQL zaokruzi na ceo broj
            builder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            builder.Entity<Invoice>().Property(i => i.TotalAmount).HasPrecision(18, 2);
            builder.Entity<InvoiceItem>().Property(s => s.UnitPrice).HasPrecision(18, 2);
            builder.Entity<InvoiceItem>().Property(s => s.TotalAmount).HasPrecision(18, 2);

            builder.Entity<InvoiceItem>().Property(s => s.Description).HasMaxLength(200).IsRequired();

            //brisanjem fakture brisu se i njene stavke
            builder.Entity<Invoice>()
                .HasMany(i => i.Items)
                .WithOne(s => s.Invoice)
                .HasForeignKey(s => s.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            //brisanjem fakture brisu se i uplate po njoj
            builder.Entity<Invoice>()
                .HasMany(i => i.Payments)
                .WithOne(p => p.Invoice)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            //inicijalni podaci
            //
            // Projects/Users referenced here are seeded by ProjectService/UserService - see
            // ProjectContext for the canonical Id values:
            //   project1 (a1b2c3d4-...) "Project Management System", Active - billed, not yet paid
            //   project2 (a2000000-...-002) "Mobile Banking App", OnHold - payment attempt failed
            //   project3 (a3000000-...-003) "E-Commerce Platform Redesign", Completed - paid in full
            var project1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            var project2 = Guid.Parse("a2000000-0000-0000-0000-000000000002");
            var project3 = Guid.Parse("a3000000-0000-0000-0000-000000000003");

            var userAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userPm = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userClient = Guid.Parse("44444444-4444-4444-4444-444444444444");

            var invoice1 = Guid.Parse("e1000001-0000-0000-0000-000000000001");
            var invoice2 = Guid.Parse("e1000002-0000-0000-0000-000000000001");
            var invoice3 = Guid.Parse("e1000003-0000-0000-0000-000000000001");

            builder.Entity<Invoice>().HasData(
                new Invoice
                {
                    InvoiceId = invoice1,
                    ProjectId = project1,
                    IssueDate = new DateTime(2026, 8, 1),
                    TotalAmount = 4200.00m,
                    Status = InvoiceStatus.Unpaid,
                    IssuedByUserId = userPm
                },
                new Invoice
                {
                    InvoiceId = invoice2,
                    ProjectId = project2,
                    IssueDate = new DateTime(2025, 4, 1),
                    TotalAmount = 9000.00m,
                    Status = InvoiceStatus.Unpaid,
                    IssuedByUserId = userPm
                },
                new Invoice
                {
                    InvoiceId = invoice3,
                    ProjectId = project3,
                    IssueDate = new DateTime(2025, 6, 15),
                    TotalAmount = 15000.00m,
                    Status = InvoiceStatus.Paid,
                    IssuedByUserId = userPm
                }
            );

            builder.Entity<InvoiceItem>().HasData(
                // Invoice 1 - project1, sprints 1-2 (Core CRUD Module).
                new InvoiceItem
                {
                    InvoiceItemId = Guid.Parse("e2000001-0000-0000-0000-000000000001"),
                    InvoiceId = invoice1,
                    Description = "Core CRUD Module - development (sprints 1-2)",
                    UnitPrice = 60.00m,
                    Quantity = 70,
                    TotalAmount = 4200.00m
                },
                // Invoice 2 - project2, security audit milestone work.
                new InvoiceItem
                {
                    InvoiceItemId = Guid.Parse("e2000002-0000-0000-0000-000000000001"),
                    InvoiceId = invoice2,
                    Description = "Security audit and remediation",
                    UnitPrice = 90.00m,
                    Quantity = 100,
                    TotalAmount = 9000.00m
                },
                // Invoice 3 - project3, final delivery.
                new InvoiceItem
                {
                    InvoiceItemId = Guid.Parse("e2000003-0000-0000-0000-000000000001"),
                    InvoiceId = invoice3,
                    Description = "Checkout Redesign - development and QA",
                    UnitPrice = 75.00m,
                    Quantity = 120,
                    TotalAmount = 9000.00m
                },
                new InvoiceItem
                {
                    InvoiceItemId = Guid.Parse("e2000003-0000-0000-0000-000000000002"),
                    InvoiceId = invoice3,
                    Description = "Project management and delivery",
                    UnitPrice = 100.00m,
                    Quantity = 60,
                    TotalAmount = 6000.00m
                }
            );

            builder.Entity<Payment>().HasData(
                // project2 - the transfer failed, so the invoice above is still sitting
                // unpaid while the project is on hold.
                new Payment
                {
                    PaymentId = Guid.Parse("e3000002-0000-0000-0000-000000000001"),
                    InvoiceId = invoice2,
                    Amount = 9000.00m,
                    Status = PaymentStatus.Failed,
                    Date = new DateTime(2025, 4, 10),
                    PaidByUserId = userAdmin
                },
                // project3 - fully paid after the redesign shipped.
                new Payment
                {
                    PaymentId = Guid.Parse("e3000003-0000-0000-0000-000000000001"),
                    InvoiceId = invoice3,
                    Amount = 15000.00m,
                    Status = PaymentStatus.Completed,
                    Date = new DateTime(2025, 6, 20),
                    PaidByUserId = userClient
                }
            );
        }
    }
}
