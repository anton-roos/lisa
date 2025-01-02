using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class CareGroupMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_CareGroup_CareGroupId",
                table: "Learners");

            migrationBuilder.DropTable(
                name: "CareGroup");

            migrationBuilder.CreateTable(
                name: "CareGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareGroups_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareGroups_Name",
                table: "CareGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareGroups_SchoolId",
                table: "CareGroups",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_CareGroups_CareGroupId",
                table: "Learners",
                column: "CareGroupId",
                principalTable: "CareGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_CareGroups_CareGroupId",
                table: "Learners");

            migrationBuilder.DropTable(
                name: "CareGroups");

            migrationBuilder.CreateTable(
                name: "CareGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareGroup_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareGroup_Name",
                table: "CareGroup",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareGroup_SchoolId",
                table: "CareGroup",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_CareGroup_CareGroupId",
                table: "Learners",
                column: "CareGroupId",
                principalTable: "CareGroup",
                principalColumn: "Id");
        }
    }
}
