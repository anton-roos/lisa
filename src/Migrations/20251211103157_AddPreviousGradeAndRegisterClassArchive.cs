using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousGradeAndRegisterClassArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "RegisterClasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousRegisterClassId",
                table: "Learners",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousSchoolGradeId",
                table: "Learners",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Learners_PreviousRegisterClassId",
                table: "Learners",
                column: "PreviousRegisterClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_PreviousSchoolGradeId",
                table: "Learners",
                column: "PreviousSchoolGradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_RegisterClasses_PreviousRegisterClassId",
                table: "Learners",
                column: "PreviousRegisterClassId",
                principalTable: "RegisterClasses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_SchoolGrades_PreviousSchoolGradeId",
                table: "Learners",
                column: "PreviousSchoolGradeId",
                principalTable: "SchoolGrades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_RegisterClasses_PreviousRegisterClassId",
                table: "Learners");

            migrationBuilder.DropForeignKey(
                name: "FK_Learners_SchoolGrades_PreviousSchoolGradeId",
                table: "Learners");

            migrationBuilder.DropIndex(
                name: "IX_Learners_PreviousRegisterClassId",
                table: "Learners");

            migrationBuilder.DropIndex(
                name: "IX_Learners_PreviousSchoolGradeId",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "PreviousRegisterClassId",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "PreviousSchoolGradeId",
                table: "Learners");
        }
    }
}
