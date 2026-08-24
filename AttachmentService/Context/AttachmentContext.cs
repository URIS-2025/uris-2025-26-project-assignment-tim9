using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AttachmentService.Models;
using AttachmentService.Models.Enums;

namespace AttachmentService.Context
{
    public class AttachmentContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AttachmentContext(DbContextOptions<AttachmentContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        // tabela
        public DbSet<Attachment> Attachments { get; set; }

        // konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("AttachmentDB");
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }

        // inicijalni podaci
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var seedTaskId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var seedUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Attachment>().HasData(
                new Attachment
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FileName = "project-charter.pdf",
                    OriginalFileName = "Project Charter.pdf",
                    StoragePath = $"projects/{seedProjectId}/33333333-3333-3333-3333-333333333333.pdf",
                    ContentType = "application/pdf",
                    FileSize = 204_800,
                    Checksum = null,
                    CreatedAt = seededAt,
                    Description = "Seed data - attached directly to the project.",
                    Status = AttachmentStatus.Ready,
                    DeletedAt = null,
                    ProjectId = seedProjectId,
                    TaskId = null,
                    UploadedByUserId = seedUserId
                },
                new Attachment
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    FileName = "bug-screenshot.png",
                    OriginalFileName = "Screenshot 2026-01-01.png",
                    StoragePath = $"projects/{seedProjectId}/tasks/{seedTaskId}/44444444-4444-4444-4444-444444444444.png",
                    ContentType = "image/png",
                    FileSize = 102_400,
                    Checksum = null,
                    CreatedAt = seededAt,
                    Description = "Seed data - attached to a task, should also show up when listing the parent project's attachments.",
                    Status = AttachmentStatus.Ready,
                    DeletedAt = null,
                    ProjectId = seedProjectId,
                    TaskId = seedTaskId,
                    UploadedByUserId = seedUserId
                }
            );
        }
    }
}
