using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkPackageService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealisticDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Backlogs",
                columns: new[] { "BacklogId", "CreatedAt", "CreatedBy", "Description", "Name", "ProjectId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("60000001-0000-0000-0000-000000000001"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("22222222-2222-2222-2222-222222222222"), "Unscheduled work for the Project Management System project.", "Project Management System Backlog", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), null },
                    { new Guid("60000003-0000-0000-0000-000000000001"), new DateTime(2024, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("22222222-2222-2222-2222-222222222222"), "Unscheduled work for the E-Commerce Platform Redesign project.", "E-Commerce Platform Redesign Backlog", new Guid("a3000000-0000-0000-0000-000000000003"), null }
                });

            migrationBuilder.InsertData(
                table: "WorkPackages",
                columns: new[] { "WorkPackageId", "CreatedAt", "Deadline", "Description", "Name", "ProjectId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("70000001-0000-0000-0000-000000000001"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Project, milestone and member management CRUD endpoints and UI.", "Core CRUD Module", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 3, new DateTime(2026, 6, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("70000001-0000-0000-0000-000000000002"), new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sprint board, task tracking and reporting/notification features.", "Sprint & Reporting Module", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 1, new DateTime(2026, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("70000003-0000-0000-0000-000000000001"), new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "One-page checkout flow with saved payment methods.", "Checkout Redesign", new Guid("a3000000-0000-0000-0000-000000000003"), 3, new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "TaskId", "ApproverId", "AssigneeId", "CreatedAt", "Description", "DueDate", "ParentTaskId", "Priority", "SprintId", "Status", "Title", "UpdatedAt", "WorkPackageId" },
                values: new object[,]
                {
                    { new Guid("80000001-0000-0000-0000-000000000001"), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "REST endpoints for creating, updating and deleting projects and milestones.", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new Guid("50000001-0000-0000-0000-000000000001"), 3, "Implement Project & Milestone CRUD API", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("70000001-0000-0000-0000-000000000001") },
                    { new Guid("80000001-0000-0000-0000-000000000002"), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Frontend screens for inviting, activating and removing project members.", new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, new Guid("50000001-0000-0000-0000-000000000002"), 3, "Build project member management UI", new DateTime(2026, 6, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("70000001-0000-0000-0000-000000000001") },
                    { new Guid("80000001-0000-0000-0000-000000000003"), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Drag-and-drop sprint board plus a burndown chart per sprint.", new DateTime(2026, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new Guid("50000001-0000-0000-0000-000000000003"), 1, "Build sprint board & burndown view", new DateTime(2026, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("70000001-0000-0000-0000-000000000002") },
                    { new Guid("80000003-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Single-page checkout with saved payment methods, replacing the old 3-step flow.", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new Guid("50000003-0000-0000-0000-000000000001"), 3, "Rebuild one-page checkout flow", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("70000003-0000-0000-0000-000000000001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Backlogs",
                keyColumn: "BacklogId",
                keyValue: new Guid("60000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Backlogs",
                keyColumn: "BacklogId",
                keyValue: new Guid("60000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("80000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("80000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("80000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "TaskId",
                keyValue: new Guid("80000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "WorkPackages",
                keyColumn: "WorkPackageId",
                keyValue: new Guid("70000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "WorkPackages",
                keyColumn: "WorkPackageId",
                keyValue: new Guid("70000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "WorkPackages",
                keyColumn: "WorkPackageId",
                keyValue: new Guid("70000003-0000-0000-0000-000000000001"));
        }
    }
}
