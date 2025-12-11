using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicEntityInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "Combinations");

            migrationBuilder.RenameColumn(
                name: "LeaveEarlyId",
                table: "LeaveEarlies",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CombinationType",
                table: "Combinations",
                newName: "Type");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "ResultSets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ResultSets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ResultSets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ResultSets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ResultSets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ResultSets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Results",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Results",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "RegisterClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RegisterClasses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RegisterClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RegisterClasses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "RegisterClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RegisterClasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RegisterClasses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "RegisterClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "LeaveEarlies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LeaveEarlies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LeaveEarlies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LeaveEarlies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LeaveEarlies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LeaveEarlies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LeaveEarlies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LeaveEarlies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LearnerSubjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "LearnerSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LearnerSubjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LearnerSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LearnerSubjects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LearnerSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LearnerSubjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LearnerSubjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LearnerSubjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "LearnerAcademicRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LearnerAcademicRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LearnerAcademicRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LearnerAcademicRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LearnerAcademicRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LearnerAcademicRecords",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LearnerAcademicRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Combinations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Combinations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Combinations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Combinations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Combinations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Combinations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Attendances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "AcademicDevelopmentClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AcademicDevelopmentClasses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "AcademicDevelopmentClasses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AcademicDevelopmentClasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Initialize AcademicYear records for each school with the current year (2025)
            migrationBuilder.Sql(@"
                INSERT INTO ""AcademicYears"" (""Id"", ""Year"", ""SchoolId"", ""IsCurrent"", ""CreatedAt"")
                SELECT gen_random_uuid(), 2025, ""Id"", true, NOW()
                FROM ""Schools"";
            ");

            // Update all existing records to link to the new AcademicYear
            // ResultSets are linked to School through SchoolGrade
            migrationBuilder.Sql(@"
                UPDATE ""ResultSets"" rs
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""SchoolGrades"" sg ON sg.""SchoolId"" = ay.""SchoolId""
                WHERE rs.""SchoolGradeId"" = sg.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Results"" r
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                WHERE r.""LearnerId"" = l.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""RegisterClasses"" rc
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""SchoolGrades"" sg ON sg.""SchoolId"" = ay.""SchoolId""
                WHERE rc.""SchoolGradeId"" = sg.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""LeaveEarlies"" le
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                WHERE le.""LearnerId"" = l.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""LearnerSubjects"" ls
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                WHERE ls.""LearnerId"" = l.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""LearnerAcademicRecords"" lar
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                WHERE lar.""LearnerId"" = l.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Combinations"" c
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                JOIN ""SchoolGrades"" sg ON sg.""SchoolId"" = ay.""SchoolId""
                WHERE c.""SchoolGradeId"" = sg.""Id"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Attendances"" a
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                WHERE a.""SchoolId"" = ay.""SchoolId"" AND ay.""Year"" = 2025;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""AcademicDevelopmentClasses"" adc
                SET ""AcademicYearId"" = ay.""Id""
                FROM ""AcademicYears"" ay
                WHERE adc.""SchoolId"" = ay.""SchoolId"" AND ay.""Year"" = 2025;
            ");

            // Generate unique IDs for all LearnerSubjects rows before creating the primary key
            migrationBuilder.Sql(@"
                UPDATE ""LearnerSubjects"" SET ""Id"" = gen_random_uuid();
            ");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ResultSets_AcademicYearId",
                table: "ResultSets",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_AcademicYearId",
                table: "Results",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_AcademicYearId",
                table: "RegisterClasses",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEarlies_AcademicYearId",
                table: "LeaveEarlies",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerSubjects_AcademicYearId",
                table: "LearnerSubjects",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerSubjects_LearnerId_SubjectId",
                table: "LearnerSubjects",
                columns: new[] { "LearnerId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAcademicRecords_AcademicYearId",
                table: "LearnerAcademicRecords",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Combinations_AcademicYearId",
                table: "Combinations",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AcademicYearId",
                table: "Attendances",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicDevelopmentClasses_AcademicYearId",
                table: "AcademicDevelopmentClasses",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicDevelopmentClasses_AcademicYears_AcademicYearId",
                table: "AcademicDevelopmentClasses",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AcademicYears_AcademicYearId",
                table: "Attendances",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combinations_AcademicYears_AcademicYearId",
                table: "Combinations",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerAcademicRecords_AcademicYears_AcademicYearId",
                table: "LearnerAcademicRecords",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerSubjects_AcademicYears_AcademicYearId",
                table: "LearnerSubjects",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveEarlies_AcademicYears_AcademicYearId",
                table: "LeaveEarlies",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisterClasses_AcademicYears_AcademicYearId",
                table: "RegisterClasses",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_AcademicYears_AcademicYearId",
                table: "Results",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultSets_AcademicYears_AcademicYearId",
                table: "ResultSets",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicDevelopmentClasses_AcademicYears_AcademicYearId",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AcademicYears_AcademicYearId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Combinations_AcademicYears_AcademicYearId",
                table: "Combinations");

            migrationBuilder.DropForeignKey(
                name: "FK_LearnerAcademicRecords_AcademicYears_AcademicYearId",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_LearnerSubjects_AcademicYears_AcademicYearId",
                table: "LearnerSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveEarlies_AcademicYears_AcademicYearId",
                table: "LeaveEarlies");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterClasses_AcademicYears_AcademicYearId",
                table: "RegisterClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_AcademicYears_AcademicYearId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultSets_AcademicYears_AcademicYearId",
                table: "ResultSets");

            migrationBuilder.DropIndex(
                name: "IX_ResultSets_AcademicYearId",
                table: "ResultSets");

            migrationBuilder.DropIndex(
                name: "IX_Results_AcademicYearId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_RegisterClasses_AcademicYearId",
                table: "RegisterClasses");

            migrationBuilder.DropIndex(
                name: "IX_LeaveEarlies_AcademicYearId",
                table: "LeaveEarlies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects");

            migrationBuilder.DropIndex(
                name: "IX_LearnerSubjects_AcademicYearId",
                table: "LearnerSubjects");

            migrationBuilder.DropIndex(
                name: "IX_LearnerSubjects_LearnerId_SubjectId",
                table: "LearnerSubjects");

            migrationBuilder.DropIndex(
                name: "IX_LearnerAcademicRecords_AcademicYearId",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropIndex(
                name: "IX_Combinations_AcademicYearId",
                table: "Combinations");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_AcademicYearId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_AcademicDevelopmentClasses_AcademicYearId",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LeaveEarlies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LearnerSubjects");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LearnerAcademicRecords");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AcademicDevelopmentClasses");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "LeaveEarlies",
                newName: "LeaveEarlyId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Combinations",
                newName: "CombinationType");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "ResultSets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "Results",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "LearnerAcademicRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "Combinations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearnerSubjects",
                table: "LearnerSubjects",
                columns: new[] { "LearnerId", "SubjectId" });
        }
    }
}
