using Microsoft.EntityFrameworkCore;
using ProjectService.Models;
using ProjectService.Models.Enums;

namespace ProjectService.Context
{
    public class ProjectContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ProjectContext(
           DbContextOptions<ProjectContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        // Tabele
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Requirement> Requirements { get; set; }

        // Konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("ProjectDB");
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }

        // Inicijalni podaci
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Project>().HasData(
        new Project
        {
            ProjectId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Name = "Project Management System",
            Budget = 10000,
            Status = ProjectStatus.Active,
            Deadline = DateTime.Parse("2026-12-31T00:00:00"),  
            CreatedAt = DateTime.Parse("2025-01-01T00:00:00")
        }
    );

            builder.Entity<ProjectMember>().HasData(
                new ProjectMember
                {
                    ProjectMemberId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                    ProjectId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    UserId = Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234"),
                    JoinedAt = DateTime.Parse("2025-01-01T00:00:00"),
                    Status = true
                }
            );

            builder.Entity<Milestone>().HasData(
                new Milestone
                {
                    MilestoneId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                    ProjectId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    ExpectedDate = DateTime.Parse("2025-06-01T00:00:00")
                }
            );

            builder.Entity<Requirement>().HasData(
                new Requirement
                {
                    RequirementId = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123"),
                    ProjectId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    Description = "Initial project requirements"
                }
            );
        }
    }
}