using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimelogService.Migrations
{
    /// <inheritdoc />
    public partial class AddLoggedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LoggedByUserId",
                table: "Timelogs",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "Timelogs",
                keyColumn: "Id",
                keyValue: new Guid("7a411c13-a195-48f7-8dbd-67596c3974c0"),
                column: "LoggedByUserId",
                value: new Guid("55555555-5555-5555-5555-555555555555"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoggedByUserId",
                table: "Timelogs");
        }
    }
}
