using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SprintService.Models;
using SprintService.Models.Enums;

namespace SprintService.Context
{
    public class SprintContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public SprintContext(DbContextOptions<SprintContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        // tabela
        public DbSet<Sprint> Sprints { get; set; }

        // konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("SprintDB");
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }

        // inicijalni podaci
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Sprint>().HasData(new Sprint
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProjectId = Guid.Parse("044f3de0-a9dd-4c2e-b745-89976a1b2a36"),
                Name = "Sprint 1",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 4, 1),
                EndDate = new DateTime(2026, 4, 15)
            });
        }
    }
}