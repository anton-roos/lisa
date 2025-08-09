using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "CareGroupUser",
                columns: table => new
                {
                    CareGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareGroupUser", x => new { x.CareGroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CareGroupUser_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CareGroupUser_CareGroups_CareGroupId",
                        column: x => x.CareGroupId,
                        principalTable: "CareGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareGroupUser_UserId",
                table: "CareGroupUser",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareGroupUser");

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
    }
}
