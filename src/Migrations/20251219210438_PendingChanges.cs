using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "TeachingPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "TeachingPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentVersion",
                table: "TeachingPlans",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "TeachingPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TeachingPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "TeachingPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicPlanHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlanHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_academicplanhistory_plan",
                table: "AcademicPlanHistories",
                column: "AcademicPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicPlanHistories");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "CurrentVersion",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TeachingPlans");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "TeachingPlans");
        }
    }
}
