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
        }
    }
}
