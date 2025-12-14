using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddAdiMultiSelectSubjectsTeachers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "AcademicDevelopmentClasses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "AdiSubjects",
                columns: table => new
                {
                    AcademicDevelopmentClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdiSubjects", x => new { x.AcademicDevelopmentClassId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_AdiSubjects_AcademicDevelopmentClasses_AcademicDevelopmentC~",
                        column: x => x.AcademicDevelopmentClassId,
                        principalTable: "AcademicDevelopmentClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdiSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdiTeachers",
                columns: table => new
                {
                    AcademicDevelopmentClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdiTeachers", x => new { x.AcademicDevelopmentClassId, x.TeacherId });
                    table.ForeignKey(
                        name: "FK_AdiTeachers_AcademicDevelopmentClasses_AcademicDevelopmentC~",
                        column: x => x.AcademicDevelopmentClassId,
                        principalTable: "AcademicDevelopmentClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdiTeachers_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdiSubjects_SubjectId",
                table: "AdiSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AdiTeachers_TeacherId",
                table: "AdiTeachers",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdiSubjects");

            migrationBuilder.DropTable(
                name: "AdiTeachers");

            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "AcademicDevelopmentClasses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
