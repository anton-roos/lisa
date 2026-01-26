using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add Notes field - Link columns were already removed in RemoveDocumentLinkFields migration
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AcademicPlanHistories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only remove Notes field - Link columns restoration is handled by RemoveDocumentLinkFields migration
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AcademicPlanHistories");
        }
    }
}
