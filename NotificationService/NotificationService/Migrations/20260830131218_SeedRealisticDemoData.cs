using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealisticDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "Description", "IsRead", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("f0000001-0000-0000-0000-000000000001"), new DateTime(2026, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "You were assigned to task 'Build sprint board & burndown view' on Project Management System.", false, "TaskAssigned", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("f0000001-0000-0000-0000-000000000002"), new DateTime(2026, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint 'Sprint 3 - Reporting & Notifications' started on Project Management System.", true, "SprintStarted", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("f0000001-0000-0000-0000-000000000003"), new DateTime(2026, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Invoice for Mobile Banking App is overdue - the last payment attempt failed.", false, "InvoiceOverdue", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("f0000001-0000-0000-0000-000000000004"), new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Your project 'E-Commerce Platform Redesign' has been marked as completed.", true, "ProjectCompleted", new Guid("44444444-4444-4444-4444-444444444444") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("f0000001-0000-0000-0000-000000000004"));
        }
    }
}
