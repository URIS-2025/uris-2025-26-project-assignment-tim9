using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttachmentService.Migrations
{
    /// <inheritdoc />
    public partial class RenameWorkPackageIdToTaskId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkPackageId",
                table: "Attachments",
                newName: "TaskId");

            migrationBuilder.UpdateData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Description", "StoragePath" },
                values: new object[] { "Seed data - attached to a task, should also show up when listing the parent project's attachments.", "projects/11111111-1111-1111-1111-111111111111/tasks/22222222-2222-2222-2222-222222222222/44444444-4444-4444-4444-444444444444.png" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Attachments",
                newName: "WorkPackageId");

            migrationBuilder.UpdateData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Description", "StoragePath" },
                values: new object[] { "Seed data - attached to a workpackage, should also show up when listing the parent project's attachments.", "projects/11111111-1111-1111-1111-111111111111/workpackages/22222222-2222-2222-2222-222222222222/44444444-4444-4444-4444-444444444444.png" });
        }
    }
}
