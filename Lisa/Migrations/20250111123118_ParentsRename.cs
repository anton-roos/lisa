using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class ParentsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerParents_Learners_LearnerId",
                table: "LearnerParents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearnerParents",
                table: "LearnerParents");

            migrationBuilder.RenameTable(
                name: "LearnerParents",
                newName: "Parents");

            migrationBuilder.RenameIndex(
                name: "IX_LearnerParents_SecondaryEmail",
                table: "Parents",
                newName: "IX_Parents_SecondaryEmail");

            migrationBuilder.RenameIndex(
                name: "IX_LearnerParents_PrimaryEmail",
                table: "Parents",
                newName: "IX_Parents_PrimaryEmail");

            migrationBuilder.RenameIndex(
                name: "IX_LearnerParents_LearnerId",
                table: "Parents",
                newName: "IX_Parents_LearnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Parents",
                table: "Parents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Learners_LearnerId",
                table: "Parents",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Learners_LearnerId",
                table: "Parents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Parents",
                table: "Parents");

            migrationBuilder.RenameTable(
                name: "Parents",
                newName: "LearnerParents");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_SecondaryEmail",
                table: "LearnerParents",
                newName: "IX_LearnerParents_SecondaryEmail");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_PrimaryEmail",
                table: "LearnerParents",
                newName: "IX_LearnerParents_PrimaryEmail");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_LearnerId",
                table: "LearnerParents",
                newName: "IX_LearnerParents_LearnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearnerParents",
                table: "LearnerParents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerParents_Learners_LearnerId",
                table: "LearnerParents",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
