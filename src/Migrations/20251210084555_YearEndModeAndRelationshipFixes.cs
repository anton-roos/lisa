using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class YearEndModeAndRelationshipFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAcademicRecords_Learners_LearnerId",
                table: "LearnerAcademicRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAcademicRecords_Learners_LearnerId",
                table: "LearnerAcademicRecords",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAcademicRecords_Learners_LearnerId",
                table: "LearnerAcademicRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAcademicRecords_Learners_LearnerId",
                table: "LearnerAcademicRecords",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
