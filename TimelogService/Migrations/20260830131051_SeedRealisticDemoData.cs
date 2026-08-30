using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TimelogService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealisticDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Timelogs",
                keyColumn: "Id",
                keyValue: new Guid("7a411c13-a195-48f7-8dbd-67596c3974c0"));

            migrationBuilder.InsertData(
                table: "Timelogs",
                columns: new[] { "Id", "Date", "HoursSpent", "LoggedByUserId", "ProjectId", "TaskId" },
                values: new object[,]
                {
                    { new Guid("90000001-0000-0000-0000-000000000001"), new DateTime(2026, 6, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 6.0, new Guid("33333333-3333-3333-3333-333333333333"), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new Guid("80000001-0000-0000-0000-000000000001") },
                    { new Guid("90000001-0000-0000-0000-000000000002"), new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.5, new Guid("33333333-3333-3333-3333-333333333333"), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new Guid("80000001-0000-0000-0000-000000000002") },
                    { new Guid("90000001-0000-0000-0000-000000000003"), new DateTime(2026, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 4.0, new Guid("33333333-3333-3333-3333-333333333333"), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new Guid("80000001-0000-0000-0000-000000000003") },
                    { new Guid("90000003-0000-0000-0000-000000000001"), new DateTime(2025, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 7.0, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a3000000-0000-0000-0000-000000000003"), new Guid("80000003-0000-0000-0000-000000000001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Timelogs",
                keyColumn: "Id",
                keyValue: new Guid("90000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Timelogs",
                keyColumn: "Id",
                keyValue: new Guid("90000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Timelogs",
                keyColumn: "Id",
                keyValue: new Guid("90000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Timelogs",
                keyColumn: "Id",
                keyValue: new Guid("90000003-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "Timelogs",
                columns: new[] { "Id", "Date", "HoursSpent", "LoggedByUserId", "ProjectId", "TaskId" },
                values: new object[] { new Guid("7a411c13-a195-48f7-8dbd-67596c3974c0"), new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 4.5, new Guid("55555555-5555-5555-5555-555555555555"), new Guid("044f3de0-a9dd-4c2e-b745-89976a1b2a36"), new Guid("21ad52f8-0281-4241-98b0-481566d25e4f") });
        }
    }
}
