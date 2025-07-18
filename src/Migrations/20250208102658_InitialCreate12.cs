using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherSubjects_UserId_SubjectId",
                table: "TeacherSubjects");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_UserId",
                table: "TeacherSubjects",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherSubjects_UserId",
                table: "TeacherSubjects");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_UserId_SubjectId",
                table: "TeacherSubjects",
                columns: new[] { "UserId", "SubjectId" },
                unique: true);
        }
    }
}
