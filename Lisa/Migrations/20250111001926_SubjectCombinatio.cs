using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class SubjectCombinatio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CombinationSubject_Combinations_SubjectCombinationId",
                table: "CombinationSubject");

            migrationBuilder.RenameColumn(
                name: "SubjectCombinationId",
                table: "CombinationSubject",
                newName: "CombinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CombinationSubject_Combinations_CombinationId",
                table: "CombinationSubject",
                column: "CombinationId",
                principalTable: "Combinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CombinationSubject_Combinations_CombinationId",
                table: "CombinationSubject");

            migrationBuilder.RenameColumn(
                name: "CombinationId",
                table: "CombinationSubject",
                newName: "SubjectCombinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CombinationSubject_Combinations_SubjectCombinationId",
                table: "CombinationSubject",
                column: "SubjectCombinationId",
                principalTable: "Combinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
