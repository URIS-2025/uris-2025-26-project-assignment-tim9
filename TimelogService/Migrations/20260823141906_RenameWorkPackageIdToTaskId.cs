using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimelogService.Migrations
{
    /// <inheritdoc />
    public partial class RenameWorkPackageIdToTaskId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkPackageId",
                table: "Timelogs",
                newName: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Timelogs",
                newName: "WorkPackageId");
        }
    }
}
