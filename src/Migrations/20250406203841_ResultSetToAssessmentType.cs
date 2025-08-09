using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class ResultSetToAssessmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssessmentType",
                table: "ResultSets",
                newName: "AssessmentTypeName");

            migrationBuilder.AlterColumn<string>(
                name: "AssessmentTopic",
                table: "ResultSets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssessmentTypeId",
                table: "ResultSets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ResultSets_AssessmentTypeId",
                table: "ResultSets",
                column: "AssessmentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultSets_AssessmentTypes_AssessmentTypeId",
                table: "ResultSets",
                column: "AssessmentTypeId",
                principalTable: "AssessmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultSets_AssessmentTypes_AssessmentTypeId",
                table: "ResultSets");

            migrationBuilder.DropIndex(
                name: "IX_ResultSets_AssessmentTypeId",
                table: "ResultSets");

            migrationBuilder.DropColumn(
                name: "AssessmentTypeId",
                table: "ResultSets");

            migrationBuilder.RenameColumn(
                name: "AssessmentTypeName",
                table: "ResultSets",
                newName: "AssessmentType");

            migrationBuilder.AlterColumn<string>(
                name: "AssessmentTopic",
                table: "ResultSets",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
