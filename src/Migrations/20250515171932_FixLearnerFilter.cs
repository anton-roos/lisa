using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class FixLearnerFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Learners_Code",
                table: "Learners");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_Code",
                table: "Learners",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Learners_Code",
                table: "Learners");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_Code",
                table: "Learners",
                column: "Code",
                unique: true);
        }
    }
}
