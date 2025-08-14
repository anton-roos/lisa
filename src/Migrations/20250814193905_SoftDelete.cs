using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Learners_LearnerId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_LearnerSubjects_Learners_LearnerId",
                table: "LearnerSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Learners_LearnerId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Learners_LearnerId",
                table: "Results");

            migrationBuilder.AddColumn<DateTime>(
                name: "DisabledAt",
                table: "Learners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisabledReason",
                table: "Learners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "Learners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Learners_LearnerId",
                table: "AttendanceRecords",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerSubjects_Learners_LearnerId",
                table: "LearnerSubjects",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Learners_LearnerId",
                table: "Parents",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Learners_LearnerId",
                table: "Results",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Learners_LearnerId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_LearnerSubjects_Learners_LearnerId",
                table: "LearnerSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Learners_LearnerId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Learners_LearnerId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "DisabledAt",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "DisabledReason",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Learners");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Learners_LearnerId",
                table: "AttendanceRecords",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LearnerSubjects_Learners_LearnerId",
                table: "LearnerSubjects",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Learners_LearnerId",
                table: "Parents",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Learners_LearnerId",
                table: "Results",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
