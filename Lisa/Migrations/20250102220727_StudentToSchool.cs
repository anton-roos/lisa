using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class StudentToSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Learners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Learners_SchoolId",
                table: "Learners",
                column: "SchoolId");

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

            migrationBuilder.DropIndex(
                name: "IX_Learners_SchoolId",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Learners");
        }
    }
}
