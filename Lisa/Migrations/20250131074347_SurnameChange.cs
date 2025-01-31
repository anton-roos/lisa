using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class SurnameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Parents",
                newName: "Surname");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Parents",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Learners",
                newName: "Surname");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Learners",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "AspNetUsers",
                newName: "Surname");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "Parents",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Parents",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "Learners",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Learners",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "AspNetUsers",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "LastName");
        }
    }
}
