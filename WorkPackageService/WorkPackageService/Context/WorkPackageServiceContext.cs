using Microsoft.EntityFrameworkCore;
using WorkPackageService.Models;
using WorkPackageService.Models.Enums;
using Task = WorkPackageService.Models.Task;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Context
{
    public class WorkPackageServiceContext : DbContext
    {
        public WorkPackageServiceContext(DbContextOptions<WorkPackageServiceContext> options)
            : base(options)
        {
        }

        public DbSet<WorkPackage> WorkPackages { get; set; }
        public DbSet<Backlog> Backlogs { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Task>()
                .HasMany(t => t.SubTasks)
                .WithOne()
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<Dependency>()
                .HasOne<Task>()
                .WithMany(t => t.Dependencies)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

           
            modelBuilder.Entity<Dependency>()
                .HasOne<Task>()
                .WithMany()
                .HasForeignKey(d => d.BlockerTaskId)
                .OnDelete(DeleteBehavior.Restrict);

         
            modelBuilder.Entity<Comment>()
                .HasOne<Task>()
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<WorkPackage>()
                .HasMany(wp => wp.Tasks)
                .WithOne()
                .HasForeignKey(t => t.WorkPackageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Inicijalni podaci.
            //
            // Projects/Sprints/Users referenced here are seeded by ProjectService,
            // SprintService and UserService respectively - see ProjectContext and
            // SprintContext for the canonical Id values:
            //   project1 (a1b2c3d4-...) "Project Management System", Active
            //   project3 (a3000000-...-003) "E-Commerce Platform Redesign", Completed
            //   sprint1/2/3 on project1, sprint1 on project3 (50000001.../50000003...)
            //   userPm (22222222-...), userMember (33333333-...)
            var project1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            var project3 = Guid.Parse("a3000000-0000-0000-0000-000000000003");

            var sprint1 = Guid.Parse("50000001-0000-0000-0000-000000000001");
            var sprint2 = Guid.Parse("50000001-0000-0000-0000-000000000002");
            var sprint3 = Guid.Parse("50000001-0000-0000-0000-000000000003");
            var sprint4 = Guid.Parse("50000003-0000-0000-0000-000000000001");

            var userAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userPm = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userMember = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var workPackage1 = Guid.Parse("70000001-0000-0000-0000-000000000001");
            var workPackage2 = Guid.Parse("70000001-0000-0000-0000-000000000002");
            var workPackage3 = Guid.Parse("70000003-0000-0000-0000-000000000001");

            modelBuilder.Entity<Backlog>().HasData(
                new Backlog
                {
                    BacklogId = Guid.Parse("60000001-0000-0000-0000-000000000001"),
                    ProjectId = project1,
                    Name = "Project Management System Backlog",
                    Description = "Unscheduled work for the Project Management System project.",
                    CreatedAt = new DateTime(2026, 1, 10),
                    CreatedBy = userPm
                },
                new Backlog
                {
                    BacklogId = Guid.Parse("60000003-0000-0000-0000-000000000001"),
                    ProjectId = project3,
                    Name = "E-Commerce Platform Redesign Backlog",
                    Description = "Unscheduled work for the E-Commerce Platform Redesign project.",
                    CreatedAt = new DateTime(2024, 9, 5),
                    CreatedBy = userPm
                }
            );

            modelBuilder.Entity<WorkPackage>().HasData(
                new WorkPackage
                {
                    WorkPackageId = workPackage1,
                    ProjectId = project1,
                    Name = "Core CRUD Module",
                    Description = "Project, milestone and member management CRUD endpoints and UI.",
                    Status = WorkPackageStatus.Completed,
                    CreatedAt = new DateTime(2026, 5, 20),
                    UpdatedAt = new DateTime(2026, 6, 28),
                    Deadline = new DateTime(2026, 6, 28)
                },
                new WorkPackage
                {
                    WorkPackageId = workPackage2,
                    ProjectId = project1,
                    Name = "Sprint & Reporting Module",
                    Description = "Sprint board, task tracking and reporting/notification features.",
                    Status = WorkPackageStatus.InProgress,
                    CreatedAt = new DateTime(2026, 6, 20),
                    UpdatedAt = new DateTime(2026, 8, 25),
                    Deadline = new DateTime(2026, 9, 14)
                },
                new WorkPackage
                {
                    WorkPackageId = workPackage3,
                    ProjectId = project3,
                    Name = "Checkout Redesign",
                    Description = "One-page checkout flow with saved payment methods.",
                    Status = WorkPackageStatus.Completed,
                    CreatedAt = new DateTime(2025, 5, 1),
                    UpdatedAt = new DateTime(2025, 6, 8),
                    Deadline = new DateTime(2025, 6, 8)
                }
            );

            modelBuilder.Entity<Task>().HasData(
                new Task
                {
                    TaskId = Guid.Parse("80000001-0000-0000-0000-000000000001"),
                    WorkPackageId = workPackage1,
                    SprintId = sprint1,
                    Title = "Implement Project & Milestone CRUD API",
                    Description = "REST endpoints for creating, updating and deleting projects and milestones.",
                    Status = TaskStatus.Done,
                    Priority = TaskPriority.High,
                    AssigneeId = userMember,
                    ApproverId = userPm,
                    DueDate = new DateTime(2026, 6, 10),
                    CreatedAt = new DateTime(2026, 6, 1),
                    UpdatedAt = new DateTime(2026, 6, 10)
                },
                new Task
                {
                    TaskId = Guid.Parse("80000001-0000-0000-0000-000000000002"),
                    WorkPackageId = workPackage1,
                    SprintId = sprint2,
                    Title = "Build project member management UI",
                    Description = "Frontend screens for inviting, activating and removing project members.",
                    Status = TaskStatus.Done,
                    Priority = TaskPriority.Medium,
                    AssigneeId = userMember,
                    ApproverId = userPm,
                    DueDate = new DateTime(2026, 6, 25),
                    CreatedAt = new DateTime(2026, 6, 15),
                    UpdatedAt = new DateTime(2026, 6, 26)
                },
                new Task
                {
                    TaskId = Guid.Parse("80000001-0000-0000-0000-000000000003"),
                    WorkPackageId = workPackage2,
                    SprintId = sprint3,
                    Title = "Build sprint board & burndown view",
                    Description = "Drag-and-drop sprint board plus a burndown chart per sprint.",
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    AssigneeId = userMember,
                    ApproverId = userPm,
                    DueDate = new DateTime(2026, 9, 5),
                    CreatedAt = new DateTime(2026, 8, 17),
                    UpdatedAt = new DateTime(2026, 8, 29)
                },
                new Task
                {
                    TaskId = Guid.Parse("80000003-0000-0000-0000-000000000001"),
                    WorkPackageId = workPackage3,
                    SprintId = sprint4,
                    Title = "Rebuild one-page checkout flow",
                    Description = "Single-page checkout with saved payment methods, replacing the old 3-step flow.",
                    Status = TaskStatus.Done,
                    Priority = TaskPriority.High,
                    AssigneeId = userPm,
                    ApproverId = userAdmin,
                    DueDate = new DateTime(2025, 6, 8),
                    CreatedAt = new DateTime(2025, 5, 26),
                    UpdatedAt = new DateTime(2025, 6, 8)
                }
            );
        }
    }
}
