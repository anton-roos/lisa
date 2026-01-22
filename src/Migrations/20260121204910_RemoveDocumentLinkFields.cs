using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentLinkFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove unused document link columns from AcademicPlanPeriods
            migrationBuilder.DropColumn(
                name: "LessonDetailLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "ClassWorkLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "HomeworkLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "HomeworkDescription",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "AssessmentLink",
                table: "AcademicPlanPeriods");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore columns if migration needs to be rolled back
            migrationBuilder.AddColumn<string>(
                name: "LessonDetailLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassWorkLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkDescription",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssessmentLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);
        }
    }
}
