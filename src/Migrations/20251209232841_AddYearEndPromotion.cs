using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddYearEndPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsYearEndMode",
                table: "Schools",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PromotionStatus",
                table: "Learners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LearnerAcademicRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    SchoolGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisterClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    CombinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubjectSnapshot = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerAcademicRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerAcademicRecords_Combinations_CombinationId",
                        column: x => x.CombinationId,
                        principalTable: "Combinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearnerAcademicRecords_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerAcademicRecords_RegisterClasses_RegisterClassId",
                        column: x => x.RegisterClassId,
                        principalTable: "RegisterClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearnerAcademicRecords_SchoolGrades_SchoolGradeId",
                        column: x => x.SchoolGradeId,
                        principalTable: "SchoolGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAcademicRecords_CombinationId",
                table: "LearnerAcademicRecords",
                column: "CombinationId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAcademicRecords_LearnerId",
                table: "LearnerAcademicRecords",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAcademicRecords_RegisterClassId",
                table: "LearnerAcademicRecords",
                column: "RegisterClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAcademicRecords_SchoolGradeId",
                table: "LearnerAcademicRecords",
                column: "SchoolGradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "IsYearEndMode",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PromotionStatus",
                table: "Learners");
        }
    }
}
