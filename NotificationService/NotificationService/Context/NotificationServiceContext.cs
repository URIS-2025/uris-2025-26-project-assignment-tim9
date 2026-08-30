using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Context
{
    public class NotificationServiceContext : DbContext
    {
        public NotificationServiceContext(DbContextOptions<NotificationServiceContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Description).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.Type).IsRequired().HasMaxLength(100);
                entity.HasIndex(n => n.UserId);
            });

            // Inicijalni podaci - Users referenced here are seeded by UserService.
            var userAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userPm = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userMember = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var userClient = Guid.Parse("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<Notification>().HasData(
                new Notification
                {
                    Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"),
                    UserId = userMember,
                    Description = "You were assigned to task 'Build sprint board & burndown view' on Project Management System.",
                    Type = "TaskAssigned",
                    IsRead = false,
                    CreatedAt = new DateTime(2026, 8, 17)
                },
                new Notification
                {
                    Id = Guid.Parse("f0000001-0000-0000-0000-000000000002"),
                    UserId = userAdmin,
                    Description = "Sprint 'Sprint 3 - Reporting & Notifications' started on Project Management System.",
                    Type = "SprintStarted",
                    IsRead = true,
                    CreatedAt = new DateTime(2026, 8, 17)
                },
                new Notification
                {
                    Id = Guid.Parse("f0000001-0000-0000-0000-000000000003"),
                    UserId = userPm,
                    Description = "Invoice for Mobile Banking App is overdue - the last payment attempt failed.",
                    Type = "InvoiceOverdue",
                    IsRead = false,
                    CreatedAt = new DateTime(2026, 8, 28)
                },
                new Notification
                {
                    Id = Guid.Parse("f0000001-0000-0000-0000-000000000004"),
                    UserId = userClient,
                    Description = "Your project 'E-Commerce Platform Redesign' has been marked as completed.",
                    Type = "ProjectCompleted",
                    IsRead = true,
                    CreatedAt = new DateTime(2026, 2, 15)
                }
            );
        }
    }
}
