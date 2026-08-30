using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRealisticDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceItems",
                keyColumn: "InvoiceItemId",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "InvoiceItems",
                keyColumn: "InvoiceItemId",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "PaymentId",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "InvoiceId",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "InvoiceId", "IssueDate", "IssuedByUserId", "ProjectId", "Status", "TotalAmount" },
                values: new object[,]
                {
                    { new Guid("e1000001-0000-0000-0000-000000000001"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 0, 4200.00m },
                    { new Guid("e1000002-0000-0000-0000-000000000001"), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a2000000-0000-0000-0000-000000000002"), 0, 9000.00m },
                    { new Guid("e1000003-0000-0000-0000-000000000001"), new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a3000000-0000-0000-0000-000000000003"), 1, 15000.00m }
                });

            migrationBuilder.InsertData(
                table: "InvoiceItems",
                columns: new[] { "InvoiceItemId", "Description", "InvoiceId", "Quantity", "TotalAmount", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("e2000001-0000-0000-0000-000000000001"), "Core CRUD Module - development (sprints 1-2)", new Guid("e1000001-0000-0000-0000-000000000001"), 70, 4200.00m, 60.00m },
                    { new Guid("e2000002-0000-0000-0000-000000000001"), "Security audit and remediation", new Guid("e1000002-0000-0000-0000-000000000001"), 100, 9000.00m, 90.00m },
                    { new Guid("e2000003-0000-0000-0000-000000000001"), "Checkout Redesign - development and QA", new Guid("e1000003-0000-0000-0000-000000000001"), 120, 9000.00m, 75.00m },
                    { new Guid("e2000003-0000-0000-0000-000000000002"), "Project management and delivery", new Guid("e1000003-0000-0000-0000-000000000001"), 60, 6000.00m, 100.00m }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Date", "InvoiceId", "PaidByUserId", "Status" },
                values: new object[,]
                {
                    { new Guid("e3000002-0000-0000-0000-000000000001"), 9000.00m, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e1000002-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), 2 },
                    { new Guid("e3000003-0000-0000-0000-000000000001"), 15000.00m, new DateTime(2025, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e1000003-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceItems",
                keyColumn: "InvoiceItemId",
                keyValue: new Guid("e2000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InvoiceItems",
                keyColumn: "InvoiceItemId",
                keyValue: new Guid("e2000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InvoiceItems",
                keyColumn: "InvoiceItemId",
                keyValue: new Guid("e2000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InvoiceItems",
                keyColumn: "InvoiceItemId",
                keyValue: new Guid("e2000003-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "PaymentId",
                keyValue: new Guid("e3000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "PaymentId",
                keyValue: new Guid("e3000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "InvoiceId",
                keyValue: new Guid("e1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "InvoiceId",
                keyValue: new Guid("e1000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "InvoiceId",
                keyValue: new Guid("e1000003-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "InvoiceId", "IssueDate", "IssuedByUserId", "ProjectId", "Status", "TotalAmount" },
                values: new object[] { new Guid("a1111111-1111-1111-1111-111111111111"), new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("55555555-5555-5555-5555-555555555555"), new Guid("044f3de0-a9dd-4c2e-b745-89976a1b2a36"), 0, 1500.00m });

            migrationBuilder.InsertData(
                table: "InvoiceItems",
                columns: new[] { "InvoiceItemId", "Description", "InvoiceId", "Quantity", "TotalAmount", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("b1111111-1111-1111-1111-111111111111"), "Analiza zahteva", new Guid("a1111111-1111-1111-1111-111111111111"), 10, 500.00m, 50.00m },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "Implementacija modula za izvestaje", new Guid("a1111111-1111-1111-1111-111111111111"), 10, 1000.00m, 100.00m }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "Date", "InvoiceId", "PaidByUserId", "Status" },
                values: new object[] { new Guid("c1111111-1111-1111-1111-111111111111"), 1500.00m, new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a1111111-1111-1111-1111-111111111111"), new Guid("66666666-6666-6666-6666-666666666666"), 0 });
        }
    }
}
