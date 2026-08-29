using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectService.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d4e5f6a7-b8c9-0123-defa-234567890123"));

            migrationBuilder.InsertData(
                table: "Milestones",
                columns: new[] { "MilestoneId", "Description", "ExpectedDate", "Name", "ProjectId" },
                values: new object[,]
                {
                    { new Guid("c0000001-0000-0000-0000-000000000001"), "All functional and non-functional requirements documented and signed off by stakeholders.", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Requirements gathering complete", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("c0000001-0000-0000-0000-000000000002"), "UI/UX mockups, database schema and API contracts finalized.", new DateTime(2025, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Design phase done", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("c0000001-0000-0000-0000-000000000003"), "Project, milestone and member management modules deployed to staging.", new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Core modules delivered", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("c0000001-0000-0000-0000-000000000004"), "Full regression suite passing; user acceptance testing approved.", new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Testing complete", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("c0000001-0000-0000-0000-000000000005"), "System live in production with monitoring and backups enabled.", new DateTime(2026, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Production release", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") }
                });

            migrationBuilder.InsertData(
                table: "ProjectMembers",
                columns: new[] { "ProjectMemberId", "JoinedAt", "ProjectId", "Status", "UserId" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), true, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("b0000001-0000-0000-0000-000000000002"), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), true, new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "ProjectId", "Budget", "CreatedAt", "Deadline", "Name", "Status" },
                values: new object[,]
                {
                    { new Guid("a2000000-0000-0000-0000-000000000002"), 45000, new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mobile Banking App", 2 },
                    { new Guid("a3000000-0000-0000-0000-000000000003"), 78000, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "E-Commerce Platform Redesign", 3 },
                    { new Guid("a4000000-0000-0000-0000-000000000004"), 22000, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2027, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Internal HR Portal", 0 }
                });

            migrationBuilder.InsertData(
                table: "Requirements",
                columns: new[] { "RequirementId", "Description", "ProjectId" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-0000-0000-000000000001"), "The system must support at least 100 concurrent authenticated users without response-time degradation.", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("d0000001-0000-0000-0000-000000000002"), "Every API endpoint must respond within 200 ms for the 95th percentile under normal load.", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("d0000001-0000-0000-0000-000000000003"), "Deleting a project must cascade to all of its milestones, requirements and members.", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("d0000001-0000-0000-0000-000000000004"), "Only users with the Admin or ProjectManager role may create or edit projects.", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                    { new Guid("d0000001-0000-0000-0000-000000000005"), "All timestamps must be stored and returned in UTC.", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") }
                });

            migrationBuilder.InsertData(
                table: "Milestones",
                columns: new[] { "MilestoneId", "Description", "ExpectedDate", "Name", "ProjectId" },
                values: new object[,]
                {
                    { new Guid("c0000002-0000-0000-0000-000000000001"), "Penetration testing completed with no critical vulnerabilities outstanding.", new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Security audit passed", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("c0000002-0000-0000-0000-000000000002"), "Card and instant-transfer rails integrated with the core banking API.", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Payments integration live", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("c0000002-0000-0000-0000-000000000003"), "Feature-complete build distributed to 200 internal testers.", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Beta release", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("c0000002-0000-0000-0000-000000000004"), "Regulatory review completed for GDPR and PSD2 requirements.", new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Compliance sign-off", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("c0000003-0000-0000-0000-000000000001"), "Stakeholder interviews and competitive analysis delivered.", new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Discovery workshop complete", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000003-0000-0000-0000-000000000002"), "Component library and brand guidelines ratified by the design board.", new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "New design system approved", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000003-0000-0000-0000-000000000003"), "One-page checkout with saved payment methods shipped.", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Checkout flow rebuilt", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000003-0000-0000-0000-000000000004"), "Largest Contentful Paint under 2s on 3G; Lighthouse score above 90.", new DateTime(2025, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance targets met", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000003-0000-0000-0000-000000000005"), "Redesigned storefront rolled out to 100% of traffic.", new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Launch", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("c0000004-0000-0000-0000-000000000001"), "Build-vs-buy decision made and single sign-on provider selected.", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vendor selection complete", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000004-0000-0000-0000-000000000002"), "Employee directory, leave requests and org chart prioritized for phase 1.", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MVP scope locked", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000004-0000-0000-0000-000000000003"), "Core self-service features available to all staff.", new DateTime(2027, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Phase 1 delivered", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000004-0000-0000-0000-000000000004"), "Performance reviews and reporting dashboards added.", new DateTime(2027, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Phase 2 delivered", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("c0000004-0000-0000-0000-000000000005"), "Legacy HR system decommissioned.", new DateTime(2027, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Full rollout", new Guid("a4000000-0000-0000-0000-000000000004") }
                });

            migrationBuilder.InsertData(
                table: "ProjectMembers",
                columns: new[] { "ProjectMemberId", "JoinedAt", "ProjectId", "Status", "UserId" },
                values: new object[,]
                {
                    { new Guid("b0000002-0000-0000-0000-000000000001"), new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a2000000-0000-0000-0000-000000000002"), true, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("b0000002-0000-0000-0000-000000000002"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a2000000-0000-0000-0000-000000000002"), true, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("b0000002-0000-0000-0000-000000000003"), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a2000000-0000-0000-0000-000000000002"), false, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("b0000003-0000-0000-0000-000000000001"), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a3000000-0000-0000-0000-000000000003"), true, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("b0000003-0000-0000-0000-000000000002"), new DateTime(2024, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a3000000-0000-0000-0000-000000000003"), true, new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("b0000004-0000-0000-0000-000000000001"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a4000000-0000-0000-0000-000000000004"), true, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("b0000004-0000-0000-0000-000000000002"), new DateTime(2026, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a4000000-0000-0000-0000-000000000004"), true, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("b0000004-0000-0000-0000-000000000003"), new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a4000000-0000-0000-0000-000000000004"), true, new Guid("44444444-4444-4444-4444-444444444444") }
                });

            migrationBuilder.InsertData(
                table: "Requirements",
                columns: new[] { "RequirementId", "Description", "ProjectId" },
                values: new object[,]
                {
                    { new Guid("d0000002-0000-0000-0000-000000000001"), "All data in transit must be encrypted using TLS 1.2 or higher.", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("d0000002-0000-0000-0000-000000000002"), "User sessions must expire after 5 minutes of inactivity.", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("d0000002-0000-0000-0000-000000000003"), "The app must remain usable offline for read-only account balance viewing.", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("d0000002-0000-0000-0000-000000000004"), "Every financial transaction must be recorded in an immutable audit log.", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("d0000002-0000-0000-0000-000000000005"), "Biometric authentication must be supported on devices that provide it.", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("d0000003-0000-0000-0000-000000000001"), "Product search must return results within 500 ms for a catalogue of 50,000 items.", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("d0000003-0000-0000-0000-000000000002"), "The storefront must be fully usable on screens as small as 320 px wide.", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("d0000003-0000-0000-0000-000000000003"), "Checkout must be completable in no more than three steps.", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("d0000003-0000-0000-0000-000000000004"), "The site must meet WCAG 2.1 AA accessibility conformance.", new Guid("a3000000-0000-0000-0000-000000000003") },
                    { new Guid("d0000004-0000-0000-0000-000000000001"), "Employees must be able to submit and track leave requests without contacting HR directly.", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("d0000004-0000-0000-0000-000000000002"), "The portal must integrate with the existing corporate single sign-on provider.", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("d0000004-0000-0000-0000-000000000003"), "A manager must be notified within one hour of a direct report submitting a request.", new Guid("a4000000-0000-0000-0000-000000000004") },
                    { new Guid("d0000004-0000-0000-0000-000000000004"), "Personally identifiable information must be accessible only to HR staff and the employee it belongs to.", new Guid("a4000000-0000-0000-0000-000000000004") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Milestones",
                keyColumn: "MilestoneId",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ProjectMembers",
                keyColumn: "ProjectMemberId",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000001-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000002-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000003-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000004-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000004-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000004-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Requirements",
                keyColumn: "RequirementId",
                keyValue: new Guid("d0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("a3000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectId",
                keyValue: new Guid("a4000000-0000-0000-0000-000000000004"));

            migrationBuilder.InsertData(
                table: "Milestones",
                columns: new[] { "MilestoneId", "Description", "ExpectedDate", "Name", "ProjectId" },
                values: new object[] { new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"), "First project milestone", new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Initial milestone", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") });

            migrationBuilder.InsertData(
                table: "ProjectMembers",
                columns: new[] { "ProjectMemberId", "JoinedAt", "ProjectId", "Status", "UserId" },
                values: new object[] { new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), true, new Guid("e5f6a7b8-c9d0-1234-efab-345678901234") });

            migrationBuilder.InsertData(
                table: "Requirements",
                columns: new[] { "RequirementId", "Description", "ProjectId" },
                values: new object[] { new Guid("d4e5f6a7-b8c9-0123-defa-234567890123"), "Initial project requirements", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890") });
        }
    }
}
