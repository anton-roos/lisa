using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class UserLinks2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Administrator_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Administrator_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Principal_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Principal_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SchoolManagement_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SchoolManagement_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SystemAdministrator_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SystemAdministrator_LastName",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Administrator_FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Administrator_LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Principal_FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Principal_LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolManagement_FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolManagement_LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemAdministrator_FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemAdministrator_LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }
    }
}
