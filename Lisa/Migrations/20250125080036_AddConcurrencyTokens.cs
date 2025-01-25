using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegisterClasses_Name",
                table: "RegisterClasses");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Parents",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Learners",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_Name",
                table: "RegisterClasses",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegisterClasses_Name",
                table: "RegisterClasses");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Learners");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_Name",
                table: "RegisterClasses",
                column: "Name",
                unique: true);
        }
    }
}
