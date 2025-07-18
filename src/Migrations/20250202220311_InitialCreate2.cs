using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "CareGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareGroups_TeacherId",
                table: "CareGroups",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_CareGroups_AspNetUsers_TeacherId",
                table: "CareGroups",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CareGroups_AspNetUsers_TeacherId",
                table: "CareGroups");

            migrationBuilder.DropIndex(
                name: "IX_CareGroups_TeacherId",
                table: "CareGroups");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "CareGroups");
        }
    }
}
