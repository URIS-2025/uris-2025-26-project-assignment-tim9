using Microsoft.EntityFrameworkCore;
using IntegrationService.Models;

namespace IntegrationService.Context
{
    public class IntegrationServiceContext : DbContext
    {
        public IntegrationServiceContext(DbContextOptions<IntegrationServiceContext> options)
            : base(options)
        {
        }

        public DbSet<Integration> Integrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Integration>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Type).IsRequired().HasMaxLength(100);
                entity.Property(i => i.ApiKeyEncrypted).IsRequired();
            });
        }
    }
}
