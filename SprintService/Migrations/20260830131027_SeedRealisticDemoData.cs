using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SprintService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealisticDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sprints",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Sprints",
                columns: new[] { "Id", "EndDate", "Name", "ProjectId", "StartDate", "Status" },
                values: new object[,]
                {
                    { new Guid("50000001-0000-0000-0000-000000000001"), new DateTime(2026, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint 1 - Core CRUD Foundations", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { new Guid("50000001-0000-0000-0000-000000000002"), new DateTime(2026, 6, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint 2 - Sprint & Task Management", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { new Guid("50000001-0000-0000-0000-000000000003"), new DateTime(2026, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint 3 - Reporting & Notifications", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2026, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { new Guid("50000003-0000-0000-0000-000000000001"), new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint 1 - Checkout Redesign", new Guid("a3000000-0000-0000-0000-000000000003"), new DateTime(2025, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sprints",
                keyColumn: "Id",
                keyValue: new Guid("50000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Sprints",
                keyColumn: "Id",
                keyValue: new Guid("50000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Sprints",
                keyColumn: "Id",
                keyValue: new Guid("50000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Sprints",
                keyColumn: "Id",
                keyValue: new Guid("50000003-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "Sprints",
                columns: new[] { "Id", "EndDate", "Name", "ProjectId", "StartDate", "Status" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint 1", new Guid("044f3de0-a9dd-4c2e-b745-89976a1b2a36"), new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });
        }
    }
}
