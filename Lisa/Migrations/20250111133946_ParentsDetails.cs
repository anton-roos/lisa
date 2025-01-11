using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class ParentsDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CellNumber",
                table: "Parents",
                newName: "SecondaryCellNumber");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryCellNumber",
                table: "Parents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CellNumber",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "Learners",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryCellNumber",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "CellNumber",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "Learners");

            migrationBuilder.RenameColumn(
                name: "SecondaryCellNumber",
                table: "Parents",
                newName: "CellNumber");
        }
    }
}
