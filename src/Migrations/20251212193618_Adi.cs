using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class Adi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcademicDevelopmentClassId",
                table: "Attendances",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AcademicDevelopmentClassId",
                table: "Attendances",
                column: "AcademicDevelopmentClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AcademicDevelopmentClasses_AcademicDevelopmentC~",
                table: "Attendances",
                column: "AcademicDevelopmentClassId",
                principalTable: "AcademicDevelopmentClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AcademicDevelopmentClasses_AcademicDevelopmentC~",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_AcademicDevelopmentClassId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AcademicDevelopmentClassId",
                table: "Attendances");
        }
    }
}
