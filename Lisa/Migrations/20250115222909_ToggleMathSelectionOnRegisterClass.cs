using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class ToggleMathSelectionOnRegisterClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MathSelectionEnabled",
                table: "RegisterClasses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MathSelectionEnabled",
                table: "RegisterClasses");
        }
    }
}
