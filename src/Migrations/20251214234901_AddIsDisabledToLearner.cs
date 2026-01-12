using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDisabledToLearner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAcademicRecords_Combinations_CombinationId",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects");

            migrationBuilder.DropIndex(
                name: "IX_LearnerSubjects_LearnerId_SubjectId",
                table: "LearnerSubjects");

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "Learners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects",
                columns: new[] { "LearnerId", "SubjectId" });

            migrationBuilder.CreateTable(
                name: "TeachingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlanWeeks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlanWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPlanWeeks_TeachingPlans_AcademicPlanId",
                        column: x => x.AcademicPlanId,
                        principalTable: "TeachingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlanPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicPlanWeekId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    Resources = table.Column<string>(type: "text", nullable: true),
                    Assessment = table.Column<string>(type: "text", nullable: true),
                    Homework = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlanPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPlanPeriods_AcademicPlanWeeks_AcademicPlanWeekId",
                        column: x => x.AcademicPlanWeekId,
                        principalTable: "AcademicPlanWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SchoolId_IsCurrent",
                table: "AcademicYears",
                columns: new[] { "SchoolId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "idx_apperiods_week",
                table: "AcademicPlanPeriods",
                column: "AcademicPlanWeekId");

            migrationBuilder.CreateIndex(
                name: "idx_apweeks_plan",
                table: "AcademicPlanWeeks",
                column: "AcademicPlanId");

            migrationBuilder.CreateIndex(
                name: "UQ_TeachingPlan_School_Grade_Subject_Teacher",
                table: "TeachingPlans",
                columns: new[] { "SchoolId", "SchoolGradeId", "SubjectId", "TeacherId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAcademicRecords_Combinations_CombinationId",
                table: "LearnerAcademicRecords",
                column: "CombinationId",
                principalTable: "Combinations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAcademicRecords_Combinations_CombinationId",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropTable(
                name: "AcademicPlanPeriods");

            migrationBuilder.DropTable(
                name: "AcademicPlanWeeks");

            migrationBuilder.DropTable(
                name: "TeachingPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_SchoolId_IsCurrent",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Learners");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerSubjects_LearnerId_SubjectId",
                table: "LearnerSubjects",
                columns: new[] { "LearnerId", "SubjectId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAcademicRecords_Combinations_CombinationId",
                table: "LearnerAcademicRecords",
                column: "CombinationId",
                principalTable: "Combinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
