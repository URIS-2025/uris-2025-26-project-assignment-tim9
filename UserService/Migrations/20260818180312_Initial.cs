using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PerformedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Details = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.LogId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salt = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactInfo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "ContactInfo", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Role", "Salt", "Username" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "+381600000001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@example.com", true, "Admin Administrator", "/2wtJodhL70wFJHy8xv+RAlJ35PWHK82KHONu0E2lpI=", 0, "Umf/S8pkKxiY2lRQjFSSpw==", "admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "+381600000002", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "pm@example.com", true, "Petar Projektni", "/2wtJodhL70wFJHy8xv+RAlJ35PWHK82KHONu0E2lpI=", 1, "Umf/S8pkKxiY2lRQjFSSpw==", "pm" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "+381600000003", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "member@example.com", true, "Marko Član", "/2wtJodhL70wFJHy8xv+RAlJ35PWHK82KHONu0E2lpI=", 2, "Umf/S8pkKxiY2lRQjFSSpw==", "member" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "+381600000004", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "client@example.com", true, "Klijent Test", "/2wtJodhL70wFJHy8xv+RAlJ35PWHK82KHONu0E2lpI=", 3, "Umf/S8pkKxiY2lRQjFSSpw==", "client" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivityLogs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
