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
        }
    }
}
