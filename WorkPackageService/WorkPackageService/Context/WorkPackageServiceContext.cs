using Microsoft.EntityFrameworkCore;
using WorkPackageService.Models;
using Task = WorkPackageService.Models.Task;

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

            // Self-referencing FK for SubTask recursion - Restrict, MySQL forbids cascade
            // when it would create multiple cascade paths on the same table.
            modelBuilder.Entity<Task>()
                .HasMany(t => t.SubTasks)
                .WithOne()
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dependency.TaskId (blocked task) - Restrict, same MySQL multiple-cascade-paths reason.
            modelBuilder.Entity<Dependency>()
                .HasOne<Task>()
                .WithMany(t => t.Dependencies)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dependency.BlockerTaskId (blocking task) - Restrict, same reason.
            modelBuilder.Entity<Dependency>()
                .HasOne<Task>()
                .WithMany()
                .HasForeignKey(d => d.BlockerTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment.TaskId - NOT NULL, Cascade (comment is existentially dependent on its task).
            modelBuilder.Entity<Comment>()
                .HasOne<Task>()
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // WorkPackage -> Task (1-N) - Cascade, tasks go with their WorkPackage.
            modelBuilder.Entity<WorkPackage>()
                .HasMany(wp => wp.Tasks)
                .WithOne()
                .HasForeignKey(t => t.WorkPackageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
