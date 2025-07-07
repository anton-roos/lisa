using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "ResultSets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultSets_TeacherId",
                table: "ResultSets",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultSets_AspNetUsers_TeacherId",
                table: "ResultSets",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultSets_AspNetUsers_TeacherId",
                table: "ResultSets");

            migrationBuilder.DropIndex(
                name: "IX_ResultSets_TeacherId",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "ResultSets");
        }
    }
}
