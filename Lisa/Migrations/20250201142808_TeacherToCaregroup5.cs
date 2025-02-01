using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class TeacherToCaregroup5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Results",
                type: "numeric(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AddColumn<string>(
                name: "Abbreviation",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Principal_Abbreviation",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abbreviation",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Principal_Abbreviation",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Results",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);
        }
    }
}
