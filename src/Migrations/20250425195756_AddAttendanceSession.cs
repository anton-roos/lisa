using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_RecordedByUserId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_LearnerId",
                table: "Attendances");

            migrationBuilder.AddColumn<Guid>(
                name: "AttendanceSessionId",
                table: "Attendances",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AttendanceSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceSessions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AttendanceSessions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AttendanceSessionId",
                table: "Attendances",
                column: "AttendanceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_LearnerId_Date",
                table: "Attendances",
                columns: new[] { "LearnerId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSessions_CreatedByUserId",
                table: "AttendanceSessions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSessions_SchoolId_Date",
                table: "AttendanceSessions",
                columns: new[] { "SchoolId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_RecordedByUserId",
                table: "Attendances",
                column: "RecordedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AttendanceSessions_AttendanceSessionId",
                table: "Attendances",
                column: "AttendanceSessionId",
                principalTable: "AttendanceSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_RecordedByUserId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AttendanceSessions_AttendanceSessionId",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "AttendanceSessions");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_AttendanceSessionId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_LearnerId_Date",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AttendanceSessionId",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_LearnerId",
                table: "Attendances",
                column: "LearnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_RecordedByUserId",
                table: "Attendances",
                column: "RecordedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
