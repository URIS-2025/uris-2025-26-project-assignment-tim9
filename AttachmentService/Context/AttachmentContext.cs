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
        //
        // Projects/Tasks/Users referenced here are seeded by ProjectService,
        // WorkPackageService and UserService - see WorkPackageServiceContext for the
        // canonical Task Id values these must line up with. Note: these rows only seed
        // metadata - the underlying objects don't exist in MinIO, so downloading them
        // will 404 even though they list fine.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var project1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            var project2 = Guid.Parse("a2000000-0000-0000-0000-000000000002");
            var project3 = Guid.Parse("a3000000-0000-0000-0000-000000000003");

            var task3 = Guid.Parse("80000001-0000-0000-0000-000000000003");
            var task4 = Guid.Parse("80000003-0000-0000-0000-000000000001");

            var userAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userPm = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userMember = Guid.Parse("33333333-3333-3333-3333-333333333333");

            var attachment1 = Guid.Parse("d0000001-0000-0000-0000-000000000001");
            var attachment2 = Guid.Parse("d0000001-0000-0000-0000-000000000002");
            var attachment3 = Guid.Parse("d0000003-0000-0000-0000-000000000001");
            var attachment4 = Guid.Parse("d0000002-0000-0000-0000-000000000001");

            modelBuilder.Entity<Attachment>().HasData(
                new Attachment
                {
                    Id = attachment1,
                    FileName = "requirements-spec.pdf",
                    OriginalFileName = "Requirements Specification.pdf",
                    StoragePath = $"projects/{project1}/{attachment1}.pdf",
                    ContentType = "application/pdf",
                    FileSize = 358_400,
                    Checksum = null,
                    CreatedAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Signed-off requirements specification.",
                    Status = AttachmentStatus.Ready,
                    DeletedAt = null,
                    ProjectId = project1,
                    TaskId = null,
                    UploadedByUserId = userPm
                },
                new Attachment
                {
                    Id = attachment2,
                    FileName = "sprint-board-mockup.png",
                    OriginalFileName = "Sprint Board Mockup.png",
                    StoragePath = $"projects/{project1}/tasks/{task3}/{attachment2}.png",
                    ContentType = "image/png",
                    FileSize = 128_000,
                    Checksum = null,
                    CreatedAt = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                    Description = "UI mockup for the sprint board & burndown task.",
                    Status = AttachmentStatus.Ready,
                    DeletedAt = null,
                    ProjectId = project1,
                    TaskId = task3,
                    UploadedByUserId = userMember
                },
                new Attachment
                {
                    Id = attachment3,
                    FileName = "checkout-flow-diagram.pdf",
                    OriginalFileName = "Checkout Flow Diagram.pdf",
                    StoragePath = $"projects/{project3}/tasks/{task4}/{attachment3}.pdf",
                    ContentType = "application/pdf",
                    FileSize = 204_800,
                    Checksum = null,
                    CreatedAt = new DateTime(2025, 5, 28, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Final checkout flow diagram delivered with the redesign.",
                    Status = AttachmentStatus.Ready,
                    DeletedAt = null,
                    ProjectId = project3,
                    TaskId = task4,
                    UploadedByUserId = userPm
                },
                new Attachment
                {
                    Id = attachment4,
                    FileName = "banking-compliance-report.docx",
                    OriginalFileName = "Banking Compliance Report.docx",
                    StoragePath = $"projects/{project2}/{attachment4}.docx",
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    FileSize = 92_160,
                    Checksum = null,
                    CreatedAt = new DateTime(2026, 6, 16, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Compliance sign-off report for the GDPR/PSD2 milestone.",
                    Status = AttachmentStatus.Ready,
                    DeletedAt = null,
                    ProjectId = project2,
                    TaskId = null,
                    UploadedByUserId = userAdmin
                }
            );
        }
    }
}
