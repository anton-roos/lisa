using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradesApplicable",
                table: "TeacherSubjects");

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "TeacherSubjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "TeacherSubjects");

            migrationBuilder.AddColumn<List<int>>(
                name: "GradesApplicable",
                table: "TeacherSubjects",
                type: "integer[]",
                nullable: true);
        }
    }
}
