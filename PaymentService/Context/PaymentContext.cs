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
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
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
            var invoiceId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
            var projectId = Guid.Parse("044f3de0-a9dd-4c2e-b745-89976a1b2a36");
            var projectManagerId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var clientId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            builder.Entity<Invoice>().HasData(new Invoice
            {
                InvoiceId = invoiceId,
                ProjectId = projectId,
                IssueDate = new DateTime(2026, 8, 1),
                TotalAmount = 1500.00m,
                Status = InvoiceStatus.Unpaid,
                IssuedByUserId = projectManagerId
            });

            builder.Entity<InvoiceItem>().HasData(
                new InvoiceItem
                {
                    InvoiceItemId = Guid.Parse("b1111111-1111-1111-1111-111111111111"),
                    InvoiceId = invoiceId,
                    Description = "Analiza zahteva",
                    UnitPrice = 50.00m,
                    Quantity = 10,
                    TotalAmount = 500.00m
                },
                new InvoiceItem
                {
                    InvoiceItemId = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
                    InvoiceId = invoiceId,
                    Description = "Implementacija modula za izvestaje",
                    UnitPrice = 100.00m,
                    Quantity = 10,
                    TotalAmount = 1000.00m
                });

            builder.Entity<Payment>().HasData(new Payment
            {
                PaymentId = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
                InvoiceId = invoiceId,
                Amount = 1500.00m,
                Status = PaymentStatus.Pending,
                Date = new DateTime(2026, 8, 5),
                PaidByUserId = clientId
            });
        }
    }
}
