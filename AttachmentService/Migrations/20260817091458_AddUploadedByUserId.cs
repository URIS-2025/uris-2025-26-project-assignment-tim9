using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttachmentService.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UploadedByUserId",
                table: "Attachments",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "UploadedByUserId",
                value: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.UpdateData(
                table: "Attachments",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "UploadedByUserId",
                value: new Guid("55555555-5555-5555-5555-555555555555"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "Attachments");
        }
    }
}
