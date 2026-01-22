using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AcademicPlanningComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_TeachingPlan_School_Grade_Subject_Teacher",
                table: "TeachingPlans");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "TeachingPlans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsCatchUpPlan",
                table: "TeachingPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalPlanId",
                table: "TeachingPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Term",
                table: "TeachingPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "AcademicPlanPeriods",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssessmentDescription",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssessmentLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassWorkDescription",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassWorkLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCompleted",
                table: "AcademicPlanPeriods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePlanned",
                table: "AcademicPlanPeriods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkDescription",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeworkLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LessonDetailDescription",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LessonDetailLink",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AcademicPlanPeriods",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentageCompleted",
                table: "AcademicPlanPeriods",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentagePlanned",
                table: "AcademicPlanPeriods",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubTopic",
                table: "AcademicPlanPeriods",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicLibraryDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Grade = table.Column<int>(type: "integer", nullable: true),
                    SubjectId = table.Column<int>(type: "integer", nullable: true),
                    Curriculum = table.Column<string>(type: "text", nullable: true),
                    Term = table.Column<int>(type: "integer", nullable: true),
                    Week = table.Column<int>(type: "integer", nullable: true),
                    Period = table.Column<int>(type: "integer", nullable: true),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    SubTopic = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessmentType = table.Column<string>(type: "text", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    DescriptionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalDocumentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicLibraryDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicLibraryDocuments_AcademicLibraryDocuments_OriginalD~",
                        column: x => x.OriginalDocumentId,
                        principalTable: "AcademicLibraryDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AcademicLibraryDocuments_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicLibraryDocuments_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicLibraryDocuments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcademicYearSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYearSetups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicYearSetups_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicYearSetups_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectGradePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    SchoolGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodsPerWeek = table.Column<int>(type: "integer", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectGradePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectGradePeriods_SchoolGrades_SchoolGradeId",
                        column: x => x.SchoolGradeId,
                        principalTable: "SchoolGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectGradePeriods_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectGradePeriods_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TermAssessmentPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    Term = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermAssessmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermAssessmentPlans_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermAssessmentPlans_SchoolGrades_SchoolGradeId",
                        column: x => x.SchoolGradeId,
                        principalTable: "SchoolGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermAssessmentPlans_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkCompletionReportRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCompletionReportRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCompletionReportRecipients_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkCompletionReportRecipients_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkCompletionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPeriods = table.Column<int>(type: "integer", nullable: false),
                    PlannedPeriods = table.Column<int>(type: "integer", nullable: false),
                    CompletedPeriods = table.Column<int>(type: "integer", nullable: false),
                    AveragePercentagePlanned = table.Column<decimal>(type: "numeric", nullable: false),
                    AveragePercentageCompleted = table.Column<decimal>(type: "numeric", nullable: false),
                    PeriodsBehindSchedule = table.Column<int>(type: "integer", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCompletionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCompletionReports_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkCompletionReports_TeachingPlans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalTable: "TeachingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcademicTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearSetupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicablePhases = table.Column<string>(type: "text", nullable: true),
                    IsGrade12Specific = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicTerms_AcademicYearSetups_AcademicYearSetupId",
                        column: x => x.AcademicYearSetupId,
                        principalTable: "AcademicYearSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdministrativeDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearSetupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdministrativeDays_AcademicYearSetups_AcademicYearSetupId",
                        column: x => x.AcademicYearSetupId,
                        principalTable: "AcademicYearSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearSetupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicablePhases = table.Column<string>(type: "text", nullable: true),
                    IsGrade12Specific = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamDates_AcademicYearSetups_AcademicYearSetupId",
                        column: x => x.AcademicYearSetupId,
                        principalTable: "AcademicYearSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearSetupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Holidays_AcademicYearSetups_AcademicYearSetupId",
                        column: x => x.AcademicYearSetupId,
                        principalTable: "AcademicYearSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TermAssessmentPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssessmentType = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    MarksCaptured = table.Column<bool>(type: "boolean", nullable: false),
                    IsMarksLate = table.Column<bool>(type: "boolean", nullable: false),
                    ResultSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledAssessments_ResultSets_ResultSetId",
                        column: x => x.ResultSetId,
                        principalTable: "ResultSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScheduledAssessments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledAssessments_TermAssessmentPlans_TermAssessmentPlan~",
                        column: x => x.TermAssessmentPlanId,
                        principalTable: "TermAssessmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkCompletionReportDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkCompletionReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicPlanPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    SubTopic = table.Column<string>(type: "text", nullable: true),
                    DatePlanned = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PercentagePlanned = table.Column<decimal>(type: "numeric", nullable: true),
                    DateCompleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PercentageCompleted = table.Column<decimal>(type: "numeric", nullable: true),
                    IsOnSchedule = table.Column<bool>(type: "boolean", nullable: false),
                    IsLate = table.Column<bool>(type: "boolean", nullable: false),
                    DaysBehind = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCompletionReportDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCompletionReportDetails_AcademicPlanPeriods_AcademicPla~",
                        column: x => x.AcademicPlanPeriodId,
                        principalTable: "AcademicPlanPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkCompletionReportDetails_WorkCompletionReports_WorkCompl~",
                        column: x => x.WorkCompletionReportId,
                        principalTable: "WorkCompletionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermWeeks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicTermId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermWeeks_AcademicTerms_AcademicTermId",
                        column: x => x.AcademicTermId,
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlans_AcademicYearId",
                table: "TeachingPlans",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlans_OriginalPlanId",
                table: "TeachingPlans",
                column: "OriginalPlanId");

            migrationBuilder.CreateIndex(
                name: "UQ_TeachingPlan_School_Grade_Subject_Teacher_Year_Term",
                table: "TeachingPlans",
                columns: new[] { "SchoolId", "SchoolGradeId", "SubjectId", "TeacherId", "AcademicYearId", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicLibraryDocuments_OriginalDocumentId",
                table: "AcademicLibraryDocuments",
                column: "OriginalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicLibraryDocuments_SchoolId",
                table: "AcademicLibraryDocuments",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicLibraryDocuments_SubjectId",
                table: "AcademicLibraryDocuments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicLibraryDocuments_TeacherId",
                table: "AcademicLibraryDocuments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicTerms_AcademicYearSetupId",
                table: "AcademicTerms",
                column: "AcademicYearSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearSetups_AcademicYearId",
                table: "AcademicYearSetups",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearSetups_SchoolId",
                table: "AcademicYearSetups",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeDays_AcademicYearSetupId",
                table: "AdministrativeDays",
                column: "AcademicYearSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamDates_AcademicYearSetupId",
                table: "ExamDates",
                column: "AcademicYearSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_AcademicYearSetupId",
                table: "Holidays",
                column: "AcademicYearSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledAssessment_NoDuplicateDate",
                table: "ScheduledAssessments",
                columns: new[] { "TermAssessmentPlanId", "ScheduledDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledAssessments_ResultSetId",
                table: "ScheduledAssessments",
                column: "ResultSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledAssessments_SubjectId",
                table: "ScheduledAssessments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGradePeriods_SchoolGradeId",
                table: "SubjectGradePeriods",
                column: "SchoolGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGradePeriods_SchoolId",
                table: "SubjectGradePeriods",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGradePeriods_SubjectId_SchoolGradeId",
                table: "SubjectGradePeriods",
                columns: new[] { "SubjectId", "SchoolGradeId" },
                unique: true,
                filter: "\"SchoolId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGradePeriods_SubjectId_SchoolGradeId_SchoolId",
                table: "SubjectGradePeriods",
                columns: new[] { "SubjectId", "SchoolGradeId", "SchoolId" },
                unique: true,
                filter: "\"SchoolId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TermAssessmentPlans_AcademicYearId",
                table: "TermAssessmentPlans",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_TermAssessmentPlans_SchoolGradeId",
                table: "TermAssessmentPlans",
                column: "SchoolGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TermAssessmentPlans_SchoolId_SchoolGradeId_AcademicYearId_T~",
                table: "TermAssessmentPlans",
                columns: new[] { "SchoolId", "SchoolGradeId", "AcademicYearId", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermWeeks_AcademicTermId",
                table: "TermWeeks",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompletionReportDetails_AcademicPlanPeriodId",
                table: "WorkCompletionReportDetails",
                column: "AcademicPlanPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompletionReportDetails_WorkCompletionReportId",
                table: "WorkCompletionReportDetails",
                column: "WorkCompletionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompletionReportRecipients_SchoolId_UserId",
                table: "WorkCompletionReportRecipients",
                columns: new[] { "SchoolId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompletionReportRecipients_UserId",
                table: "WorkCompletionReportRecipients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompletionReports_SchoolId",
                table: "WorkCompletionReports",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCompletionReports_TeachingPlanId",
                table: "WorkCompletionReports",
                column: "TeachingPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingPlans_AcademicYears_AcademicYearId",
                table: "TeachingPlans",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingPlans_TeachingPlans_OriginalPlanId",
                table: "TeachingPlans",
                column: "OriginalPlanId",
                principalTable: "TeachingPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeachingPlans_AcademicYears_AcademicYearId",
                table: "TeachingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TeachingPlans_TeachingPlans_OriginalPlanId",
                table: "TeachingPlans");

            migrationBuilder.DropTable(
                name: "AcademicLibraryDocuments");

            migrationBuilder.DropTable(
                name: "AdministrativeDays");

            migrationBuilder.DropTable(
                name: "ExamDates");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "ScheduledAssessments");

            migrationBuilder.DropTable(
                name: "SubjectGradePeriods");

            migrationBuilder.DropTable(
                name: "TermWeeks");

            migrationBuilder.DropTable(
                name: "WorkCompletionReportDetails");

            migrationBuilder.DropTable(
                name: "WorkCompletionReportRecipients");

            migrationBuilder.DropTable(
                name: "TermAssessmentPlans");

            migrationBuilder.DropTable(
                name: "AcademicTerms");

            migrationBuilder.DropTable(
                name: "WorkCompletionReports");

            migrationBuilder.DropTable(
                name: "AcademicYearSetups");

            migrationBuilder.DropIndex(
                name: "IX_TeachingPlans_AcademicYearId",
                table: "TeachingPlans");

            migrationBuilder.DropIndex(
                name: "IX_TeachingPlans_OriginalPlanId",
                table: "TeachingPlans");

            migrationBuilder.DropIndex(
                name: "UQ_TeachingPlan_School_Grade_Subject_Teacher_Year_Term",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "IsCatchUpPlan",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "OriginalPlanId",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "AssessmentDescription",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "AssessmentLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "ClassWorkDescription",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "ClassWorkLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "DateCompleted",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "DatePlanned",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "HomeworkDescription",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "HomeworkLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "LessonDetailDescription",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "LessonDetailLink",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "PercentageCompleted",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "PercentagePlanned",
                table: "AcademicPlanPeriods");

            migrationBuilder.DropColumn(
                name: "SubTopic",
                table: "AcademicPlanPeriods");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "AcademicPlanPeriods",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "UQ_TeachingPlan_School_Grade_Subject_Teacher",
                table: "TeachingPlans",
                columns: new[] { "SchoolId", "SchoolGradeId", "SubjectId", "TeacherId" },
                unique: true);
        }
    }
}
