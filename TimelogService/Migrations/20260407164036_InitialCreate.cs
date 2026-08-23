using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimelogService.Migrations
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
                name: "Timelogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WorkPackageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    HoursSpent = table.Column<double>(type: "double", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timelogs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Timelogs",
                columns: new[] { "Id", "Date", "HoursSpent", "ProjectId", "WorkPackageId" },
                values: new object[] { new Guid("7a411c13-a195-48f7-8dbd-67596c3974c0"), new DateTime(2026, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 4.5, new Guid("044f3de0-a9dd-4c2e-b745-89976a1b2a36"), new Guid("21ad52f8-0281-4241-98b0-481566d25e4f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Timelogs");
        }
    }
}
