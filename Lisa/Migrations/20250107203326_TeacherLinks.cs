using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class TeacherLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisterClasses_AspNetUsers_TeacherId",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RegisterClasses");

            migrationBuilder.RenameColumn(
                name: "PeriodStartTime",
                table: "Periods",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "PeriodEndTime",
                table: "Periods",
                newName: "EndTime");

            migrationBuilder.RenameIndex(
                name: "IX_Periods_TeacherId_PeriodStartTime_PeriodEndTime",
                table: "Periods",
                newName: "IX_Periods_TeacherId_StartTime_EndTime");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId1",
                table: "RegisterClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId1",
                table: "Periods",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_TeacherId1",
                table: "RegisterClasses",
                column: "TeacherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Periods_TeacherId1",
                table: "Periods",
                column: "TeacherId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Periods_AspNetUsers_TeacherId1",
                table: "Periods",
                column: "TeacherId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterClasses_AspNetUsers_TeacherId",
                table: "RegisterClasses",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterClasses_AspNetUsers_TeacherId1",
                table: "RegisterClasses",
                column: "TeacherId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Periods_AspNetUsers_TeacherId1",
                table: "Periods");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterClasses_AspNetUsers_TeacherId",
                table: "RegisterClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterClasses_AspNetUsers_TeacherId1",
                table: "RegisterClasses");

            migrationBuilder.DropIndex(
                name: "IX_RegisterClasses_TeacherId1",
                table: "RegisterClasses");

            migrationBuilder.DropIndex(
                name: "IX_Periods_TeacherId1",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "Periods");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Periods",
                newName: "PeriodStartTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Periods",
                newName: "PeriodEndTime");

            migrationBuilder.RenameIndex(
                name: "IX_Periods_TeacherId_StartTime_EndTime",
                table: "Periods",
                newName: "IX_Periods_TeacherId_PeriodStartTime_PeriodEndTime");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RegisterClasses",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterClasses_AspNetUsers_TeacherId",
                table: "RegisterClasses",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
