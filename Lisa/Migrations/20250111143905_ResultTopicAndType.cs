using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class ResultTopicAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssessmentTopic",
                table: "Results",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssessmentType",
                table: "Results",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssessmentTopic",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "AssessmentType",
                table: "Results");
        }
    }
}
