using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class StudentToSchoolCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_Schools_SchoolId",
                table: "Learners");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_Schools_SchoolId",
                table: "Learners",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_Schools_SchoolId",
                table: "Learners");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_Schools_SchoolId",
                table: "Learners",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
