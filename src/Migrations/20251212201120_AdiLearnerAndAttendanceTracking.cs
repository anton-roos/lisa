using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AdiLearnerAndAttendanceTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceStartedAt",
                table: "AcademicDevelopmentClasses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceStoppedAt",
                table: "AcademicDevelopmentClasses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAttendanceOpen",
                table: "AcademicDevelopmentClasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AdiLearners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicDevelopmentClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAdditional = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdiLearners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdiLearners_AcademicDevelopmentClasses_AcademicDevelopmentC~",
                        column: x => x.AcademicDevelopmentClassId,
                        principalTable: "AcademicDevelopmentClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdiLearners_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdiLearners_AcademicDevelopmentClassId_LearnerId",
                table: "AdiLearners",
                columns: new[] { "AcademicDevelopmentClassId", "LearnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdiLearners_LearnerId",
                table: "AdiLearners",
                column: "LearnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdiLearners");

            migrationBuilder.DropColumn(
                name: "AttendanceStartedAt",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropColumn(
                name: "AttendanceStoppedAt",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropColumn(
                name: "IsAttendanceOpen",
                table: "AcademicDevelopmentClasses");
        }
    }
}
