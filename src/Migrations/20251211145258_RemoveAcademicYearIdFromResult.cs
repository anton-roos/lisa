using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAcademicYearIdFromResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_AcademicYears_AcademicYearId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_AcademicYearId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Results");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Results",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_AcademicYearId",
                table: "Results",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_AcademicYears_AcademicYearId",
                table: "Results",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");
        }
    }
}
