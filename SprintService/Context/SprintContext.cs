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
        //
        // Sprints belong to Projects seeded by ProjectService - see ProjectContext for the
        // canonical ProjectId/UserId values these must line up with:
        //   project1 (a1b2c3d4-...) "Project Management System", Active, deadline 2026-12-31
        //   project3 (a3000000-...-003) "E-Commerce Platform Redesign", Completed, deadline 2026-02-28
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var project1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            var project3 = Guid.Parse("a3000000-0000-0000-0000-000000000003");

            builder.Entity<Sprint>().HasData(
                // project1 - two closed sprints leading up to the "Core modules delivered"
                // milestone (2026-06-30), then a third currently in flight.
                new Sprint
                {
                    Id = Guid.Parse("50000001-0000-0000-0000-000000000001"),
                    ProjectId = project1,
                    Name = "Sprint 1 - Core CRUD Foundations",
                    Status = SprintStatus.Completed,
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2026, 6, 14)
                },
                new Sprint
                {
                    Id = Guid.Parse("50000001-0000-0000-0000-000000000002"),
                    ProjectId = project1,
                    Name = "Sprint 2 - Sprint & Task Management",
                    Status = SprintStatus.Completed,
                    StartDate = new DateTime(2026, 6, 15),
                    EndDate = new DateTime(2026, 6, 28)
                },
                new Sprint
                {
                    Id = Guid.Parse("50000001-0000-0000-0000-000000000003"),
                    ProjectId = project1,
                    Name = "Sprint 3 - Reporting & Notifications",
                    Status = SprintStatus.Active,
                    StartDate = new DateTime(2026, 8, 17),
                    EndDate = new DateTime(2026, 8, 30)
                },

                // project3 - closed out before the "Checkout flow rebuilt" milestone (2025-06-10).
                new Sprint
                {
                    Id = Guid.Parse("50000003-0000-0000-0000-000000000001"),
                    ProjectId = project3,
                    Name = "Sprint 1 - Checkout Redesign",
                    Status = SprintStatus.Completed,
                    StartDate = new DateTime(2025, 5, 26),
                    EndDate = new DateTime(2025, 6, 8)
                }
            );
        }
    }
}