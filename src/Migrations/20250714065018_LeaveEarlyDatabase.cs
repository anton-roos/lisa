using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class LeaveEarlyDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaveEarlies",
                columns: table => new
                {
                    LeaveEarlyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendenceRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    LearnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SchoolGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignOutTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    PermissionType = table.Column<int>(type: "integer", nullable: false),
                    TelephonicNotes = table.Column<string>(type: "text", nullable: true),
                    PickUpType = table.Column<int>(type: "integer", nullable: false),
                    PickupFamilyMemberIDNo = table.Column<string>(type: "text", nullable: true),
                    PickupFamilyMemberFirstname = table.Column<string>(type: "text", nullable: true),
                    PickupFamilyMemberSurname = table.Column<string>(type: "text", nullable: true),
                    PickupUberTransportIDNo = table.Column<string>(type: "text", nullable: true),
                    PickupUberTransportRegNo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveEarlies", x => x.LeaveEarlyId);
                    table.ForeignKey(
                        name: "FK_LeaveEarlies_AttendanceRecords_AttendenceRecordId",
                        column: x => x.AttendenceRecordId,
                        principalTable: "AttendanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveEarlies_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveEarlies_SchoolGrades_SchoolGradeId",
                        column: x => x.SchoolGradeId,
                        principalTable: "SchoolGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEarlies_AttendenceRecordId",
                table: "LeaveEarlies",
                column: "AttendenceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEarlies_LearnerId",
                table: "LeaveEarlies",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEarlies_SchoolGradeId",
                table: "LeaveEarlies",
                column: "SchoolGradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveEarlies");
        }
    }
}
