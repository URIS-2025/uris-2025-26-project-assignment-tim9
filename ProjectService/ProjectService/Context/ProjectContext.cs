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

            var project1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            var project2 = Guid.Parse("a2000000-0000-0000-0000-000000000002");
            var project3 = Guid.Parse("a3000000-0000-0000-0000-000000000003");
            var project4 = Guid.Parse("a4000000-0000-0000-0000-000000000004");

            // UserService seed korisnici
            var userAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userPm = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userMember = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var userClient = Guid.Parse("44444444-4444-4444-4444-444444444444");

            builder.Entity<Project>().HasData(
                new Project
                {
                    ProjectId = project1,
                    Name = "Project Management System",
                    Budget = 10000,
                    Status = ProjectStatus.Active,
                    Deadline = DateTime.Parse("2026-12-31T00:00:00"),
                    CreatedAt = DateTime.Parse("2025-01-01T00:00:00")
                },
                new Project
                {
                    ProjectId = project2,
                    Name = "Mobile Banking App",
                    Budget = 45000,
                    Status = ProjectStatus.OnHold,
                    Deadline = DateTime.Parse("2026-06-30T00:00:00"),
                    CreatedAt = DateTime.Parse("2025-03-15T00:00:00")
                },
                new Project
                {
                    ProjectId = project3,
                    Name = "E-Commerce Platform Redesign",
                    Budget = 78000,
                    Status = ProjectStatus.Completed,
                    Deadline = DateTime.Parse("2026-02-28T00:00:00"),
                    CreatedAt = DateTime.Parse("2024-09-01T00:00:00")
                },
                new Project
                {
                    ProjectId = project4,
                    Name = "Internal HR Portal",
                    Budget = 22000,
                    Status = ProjectStatus.Planned,
                    Deadline = DateTime.Parse("2027-09-30T00:00:00"),
                    CreatedAt = DateTime.Parse("2026-07-01T00:00:00")
                }
            );

            builder.Entity<ProjectMember>().HasData(
                // Project 1
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000001-0000-0000-0000-000000000001"), ProjectId = project1, UserId = userPm, JoinedAt = DateTime.Parse("2025-01-01T00:00:00"), Status = true },
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000001-0000-0000-0000-000000000002"), ProjectId = project1, UserId = userMember, JoinedAt = DateTime.Parse("2025-01-05T00:00:00"), Status = true },

                // Project 2
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000002-0000-0000-0000-000000000001"), ProjectId = project2, UserId = userAdmin, JoinedAt = DateTime.Parse("2025-03-15T00:00:00"), Status = true },
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000002-0000-0000-0000-000000000002"), ProjectId = project2, UserId = userPm, JoinedAt = DateTime.Parse("2025-03-20T00:00:00"), Status = true },
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000002-0000-0000-0000-000000000003"), ProjectId = project2, UserId = userMember, JoinedAt = DateTime.Parse("2025-04-01T00:00:00"), Status = false },

                // Project 3
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000003-0000-0000-0000-000000000001"), ProjectId = project3, UserId = userPm, JoinedAt = DateTime.Parse("2024-09-01T00:00:00"), Status = true },
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000003-0000-0000-0000-000000000002"), ProjectId = project3, UserId = userClient, JoinedAt = DateTime.Parse("2024-09-10T00:00:00"), Status = true },

                // Project 4
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000004-0000-0000-0000-000000000001"), ProjectId = project4, UserId = userAdmin, JoinedAt = DateTime.Parse("2026-07-01T00:00:00"), Status = true },
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000004-0000-0000-0000-000000000002"), ProjectId = project4, UserId = userMember, JoinedAt = DateTime.Parse("2026-07-10T00:00:00"), Status = true },
                new ProjectMember { ProjectMemberId = Guid.Parse("b0000004-0000-0000-0000-000000000003"), ProjectId = project4, UserId = userClient, JoinedAt = DateTime.Parse("2026-07-15T00:00:00"), Status = true }
            );

            builder.Entity<Milestone>().HasData(
                // Project 1 (deadline 2026-12-31)
                new Milestone { MilestoneId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProjectId = project1, Name = "Requirements gathering complete", Description = "All functional and non-functional requirements documented and signed off by stakeholders.", ExpectedDate = DateTime.Parse("2025-03-01T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000001-0000-0000-0000-000000000002"), ProjectId = project1, Name = "Design phase done", Description = "UI/UX mockups, database schema and API contracts finalized.", ExpectedDate = DateTime.Parse("2025-07-15T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000001-0000-0000-0000-000000000003"), ProjectId = project1, Name = "Core modules delivered", Description = "Project, milestone and member management modules deployed to staging.", ExpectedDate = DateTime.Parse("2026-06-30T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000001-0000-0000-0000-000000000004"), ProjectId = project1, Name = "Testing complete", Description = "Full regression suite passing; user acceptance testing approved.", ExpectedDate = DateTime.Parse("2026-11-15T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000001-0000-0000-0000-000000000005"), ProjectId = project1, Name = "Production release", Description = "System live in production with monitoring and backups enabled.", ExpectedDate = DateTime.Parse("2026-12-20T00:00:00") },

                // Project 2 (deadline 2026-06-30)
                new Milestone { MilestoneId = Guid.Parse("c0000002-0000-0000-0000-000000000001"), ProjectId = project2, Name = "Security audit passed", Description = "Penetration testing completed with no critical vulnerabilities outstanding.", ExpectedDate = DateTime.Parse("2025-05-01T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), ProjectId = project2, Name = "Payments integration live", Description = "Card and instant-transfer rails integrated with the core banking API.", ExpectedDate = DateTime.Parse("2025-11-01T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000002-0000-0000-0000-000000000003"), ProjectId = project2, Name = "Beta release", Description = "Feature-complete build distributed to 200 internal testers.", ExpectedDate = DateTime.Parse("2026-03-15T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000002-0000-0000-0000-000000000004"), ProjectId = project2, Name = "Compliance sign-off", Description = "Regulatory review completed for GDPR and PSD2 requirements.", ExpectedDate = DateTime.Parse("2026-06-15T00:00:00") },

                // Project 3 (deadline 2026-02-28)
                new Milestone { MilestoneId = Guid.Parse("c0000003-0000-0000-0000-000000000001"), ProjectId = project3, Name = "Discovery workshop complete", Description = "Stakeholder interviews and competitive analysis delivered.", ExpectedDate = DateTime.Parse("2024-10-15T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000003-0000-0000-0000-000000000002"), ProjectId = project3, Name = "New design system approved", Description = "Component library and brand guidelines ratified by the design board.", ExpectedDate = DateTime.Parse("2025-01-20T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000003-0000-0000-0000-000000000003"), ProjectId = project3, Name = "Checkout flow rebuilt", Description = "One-page checkout with saved payment methods shipped.", ExpectedDate = DateTime.Parse("2025-06-10T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000003-0000-0000-0000-000000000004"), ProjectId = project3, Name = "Performance targets met", Description = "Largest Contentful Paint under 2s on 3G; Lighthouse score above 90.", ExpectedDate = DateTime.Parse("2025-11-30T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000003-0000-0000-0000-000000000005"), ProjectId = project3, Name = "Launch", Description = "Redesigned storefront rolled out to 100% of traffic.", ExpectedDate = DateTime.Parse("2026-02-15T00:00:00") },

                // Project 4 (deadline 2027-09-30)
                new Milestone { MilestoneId = Guid.Parse("c0000004-0000-0000-0000-000000000001"), ProjectId = project4, Name = "Vendor selection complete", Description = "Build-vs-buy decision made and single sign-on provider selected.", ExpectedDate = DateTime.Parse("2026-08-01T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000004-0000-0000-0000-000000000002"), ProjectId = project4, Name = "MVP scope locked", Description = "Employee directory, leave requests and org chart prioritized for phase 1.", ExpectedDate = DateTime.Parse("2026-10-01T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000004-0000-0000-0000-000000000003"), ProjectId = project4, Name = "Phase 1 delivered", Description = "Core self-service features available to all staff.", ExpectedDate = DateTime.Parse("2027-03-31T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000004-0000-0000-0000-000000000004"), ProjectId = project4, Name = "Phase 2 delivered", Description = "Performance reviews and reporting dashboards added.", ExpectedDate = DateTime.Parse("2027-07-31T00:00:00") },
                new Milestone { MilestoneId = Guid.Parse("c0000004-0000-0000-0000-000000000005"), ProjectId = project4, Name = "Full rollout", Description = "Legacy HR system decommissioned.", ExpectedDate = DateTime.Parse("2027-09-15T00:00:00") }
            );

            builder.Entity<Requirement>().HasData(
                // Project 1
                new Requirement { RequirementId = Guid.Parse("d0000001-0000-0000-0000-000000000001"), ProjectId = project1, Description = "The system must support at least 100 concurrent authenticated users without response-time degradation." },
                new Requirement { RequirementId = Guid.Parse("d0000001-0000-0000-0000-000000000002"), ProjectId = project1, Description = "Every API endpoint must respond within 200 ms for the 95th percentile under normal load." },
                new Requirement { RequirementId = Guid.Parse("d0000001-0000-0000-0000-000000000003"), ProjectId = project1, Description = "Deleting a project must cascade to all of its milestones, requirements and members." },
                new Requirement { RequirementId = Guid.Parse("d0000001-0000-0000-0000-000000000004"), ProjectId = project1, Description = "Only users with the Admin or ProjectManager role may create or edit projects." },
                new Requirement { RequirementId = Guid.Parse("d0000001-0000-0000-0000-000000000005"), ProjectId = project1, Description = "All timestamps must be stored and returned in UTC." },

                // Project 2
                new Requirement { RequirementId = Guid.Parse("d0000002-0000-0000-0000-000000000001"), ProjectId = project2, Description = "All data in transit must be encrypted using TLS 1.2 or higher." },
                new Requirement { RequirementId = Guid.Parse("d0000002-0000-0000-0000-000000000002"), ProjectId = project2, Description = "User sessions must expire after 5 minutes of inactivity." },
                new Requirement { RequirementId = Guid.Parse("d0000002-0000-0000-0000-000000000003"), ProjectId = project2, Description = "The app must remain usable offline for read-only account balance viewing." },
                new Requirement { RequirementId = Guid.Parse("d0000002-0000-0000-0000-000000000004"), ProjectId = project2, Description = "Every financial transaction must be recorded in an immutable audit log." },
                new Requirement { RequirementId = Guid.Parse("d0000002-0000-0000-0000-000000000005"), ProjectId = project2, Description = "Biometric authentication must be supported on devices that provide it." },

                // Project 3
                new Requirement { RequirementId = Guid.Parse("d0000003-0000-0000-0000-000000000001"), ProjectId = project3, Description = "Product search must return results within 500 ms for a catalogue of 50,000 items." },
                new Requirement { RequirementId = Guid.Parse("d0000003-0000-0000-0000-000000000002"), ProjectId = project3, Description = "The storefront must be fully usable on screens as small as 320 px wide." },
                new Requirement { RequirementId = Guid.Parse("d0000003-0000-0000-0000-000000000003"), ProjectId = project3, Description = "Checkout must be completable in no more than three steps." },
                new Requirement { RequirementId = Guid.Parse("d0000003-0000-0000-0000-000000000004"), ProjectId = project3, Description = "The site must meet WCAG 2.1 AA accessibility conformance." },

                // Project 4
                new Requirement { RequirementId = Guid.Parse("d0000004-0000-0000-0000-000000000001"), ProjectId = project4, Description = "Employees must be able to submit and track leave requests without contacting HR directly." },
                new Requirement { RequirementId = Guid.Parse("d0000004-0000-0000-0000-000000000002"), ProjectId = project4, Description = "The portal must integrate with the existing corporate single sign-on provider." },
                new Requirement { RequirementId = Guid.Parse("d0000004-0000-0000-0000-000000000003"), ProjectId = project4, Description = "A manager must be notified within one hour of a direct report submitting a request." },
                new Requirement { RequirementId = Guid.Parse("d0000004-0000-0000-0000-000000000004"), ProjectId = project4, Description = "Personally identifiable information must be accessible only to HR staff and the employee it belongs to." }
            );

            builder.Entity<Milestone>()
                .HasOne<Project>()
                .WithMany()
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectMember>()
                .HasOne<Project>()
                .WithMany()
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Requirement>()
                .HasOne<Project>()
                .WithMany()
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
