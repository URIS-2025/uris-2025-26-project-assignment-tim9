using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AttachmentService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealisticDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.InsertData(
                table: "Attachments",
                columns: new[] { "Id", "Checksum", "ContentType", "CreatedAt", "DeletedAt", "Description", "FileName", "FileSize", "OriginalFileName", "ProjectId", "Status", "StoragePath", "TaskId", "UploadedByUserId" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-0000-0000-000000000001"), null, "application/pdf", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Signed-off requirements specification.", "requirements-spec.pdf", 358400L, "Requirements Specification.pdf", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 2, "projects/a1b2c3d4-e5f6-7890-abcd-ef1234567890/d0000001-0000-0000-0000-000000000001.pdf", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("d0000001-0000-0000-0000-000000000002"), null, "image/png", new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "UI mockup for the sprint board & burndown task.", "sprint-board-mockup.png", 128000L, "Sprint Board Mockup.png", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 2, "projects/a1b2c3d4-e5f6-7890-abcd-ef1234567890/tasks/80000001-0000-0000-0000-000000000003/d0000001-0000-0000-0000-000000000002.png", new Guid("80000001-0000-0000-0000-000000000003"), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("d0000002-0000-0000-0000-000000000001"), null, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Utc), null, "Compliance sign-off report for the GDPR/PSD2 milestone.", "banking-compliance-report.docx", 92160L, "Banking Compliance Report.docx", new Guid("a2000000-0000-0000-0000-000000000002"), 2, "projects/a2000000-0000-0000-0000-000000000002/d0000002-0000-0000-0000-000000000001.docx", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("d0000003-0000-0000-0000-000000000001"), null, "application/pdf", new DateTime(2025, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, "Final checkout flow diagram delivered with the redesign.", "checkout-flow-diagram.pdf", 204800L, "Checkout Flow Diagram.pdf", new Guid("a3000000-0000-0000-0000-000000000003"), 2, "projects/a3000000-0000-0000-0000-000000000003/tasks/80000003-0000-0000-0000-000000000001/d0000003-0000-0000-0000-000000000001.pdf", new Guid("80000003-0000-0000-0000-000000000001"), new Guid("22222222-2222-2222-2222-222222222222") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "Attachments",
                columns: new[] { "Id", "Checksum", "ContentType", "CreatedAt", "DeletedAt", "Description", "FileName", "FileSize", "OriginalFileName", "ProjectId", "Status", "StoragePath", "TaskId", "UploadedByUserId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "application/pdf", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seed data - attached directly to the project.", "project-charter.pdf", 204800L, "Project Charter.pdf", new Guid("11111111-1111-1111-1111-111111111111"), 2, "projects/11111111-1111-1111-1111-111111111111/33333333-3333-3333-3333-333333333333.pdf", null, new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, "image/png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seed data - attached to a task, should also show up when listing the parent project's attachments.", "bug-screenshot.png", 102400L, "Screenshot 2026-01-01.png", new Guid("11111111-1111-1111-1111-111111111111"), 2, "projects/11111111-1111-1111-1111-111111111111/tasks/22222222-2222-2222-2222-222222222222/44444444-4444-4444-4444-444444444444.png", new Guid("22222222-2222-2222-2222-222222222222"), new Guid("55555555-5555-5555-5555-555555555555") }
                });
        }
    }
}
