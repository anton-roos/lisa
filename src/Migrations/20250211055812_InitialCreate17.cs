using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolGradeId",
                table: "ResultSets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultSets_SchoolGradeId",
                table: "ResultSets",
                column: "SchoolGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultSets_SchoolGrades_SchoolGradeId",
                table: "ResultSets",
                column: "SchoolGradeId",
                principalTable: "SchoolGrades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultSets_SchoolGrades_SchoolGradeId",
                table: "ResultSets");

            migrationBuilder.DropIndex(
                name: "IX_ResultSets_SchoolGradeId",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "SchoolGradeId",
                table: "ResultSets");
        }
    }
}
