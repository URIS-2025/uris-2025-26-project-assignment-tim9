using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AttachmentService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalFileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoragePath = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Checksum = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WorkPackageId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Attachments",
                columns: new[] { "Id", "Checksum", "ContentType", "CreatedAt", "DeletedAt", "Description", "FileName", "FileSize", "OriginalFileName", "ProjectId", "Status", "StoragePath", "WorkPackageId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "application/pdf", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seed data - attached directly to the project.", "project-charter.pdf", 204800L, "Project Charter.pdf", new Guid("11111111-1111-1111-1111-111111111111"), 2, "projects/11111111-1111-1111-1111-111111111111/33333333-3333-3333-3333-333333333333.pdf", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, "image/png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seed data - attached to a workpackage, should also show up when listing the parent project's attachments.", "bug-screenshot.png", 102400L, "Screenshot 2026-01-01.png", new Guid("11111111-1111-1111-1111-111111111111"), 2, "projects/11111111-1111-1111-1111-111111111111/workpackages/22222222-2222-2222-2222-222222222222/44444444-4444-4444-4444-444444444444.png", new Guid("22222222-2222-2222-2222-222222222222") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");
        }
    }
}
