using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CombinationId",
                table: "LearnerSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerSubjects_CombinationId",
                table: "LearnerSubjects",
                column: "CombinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerSubjects_Combinations_CombinationId",
                table: "LearnerSubjects",
                column: "CombinationId",
                principalTable: "Combinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerSubjects_Combinations_CombinationId",
                table: "LearnerSubjects");

            migrationBuilder.DropIndex(
                name: "IX_LearnerSubjects_CombinationId",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "CombinationId",
                table: "LearnerSubjects");
        }
    }
}
