using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedStatusSystemAndArchiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisabledAt",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "DisabledReason",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "PromotionStatus",
                table: "Learners");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "ResultSets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ResultSets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "Results",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "Combinations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Combinations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "Combinations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Combinations");

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

            migrationBuilder.AddColumn<int>(
                name: "PromotionStatus",
                table: "Learners",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
