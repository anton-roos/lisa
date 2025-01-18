using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class SMTPDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpDetails_Port",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "SmtpDetails_Password",
                table: "Schools",
                newName: "SmtpPassword");

            migrationBuilder.RenameColumn(
                name: "SmtpDetails_Host",
                table: "Schools",
                newName: "SmtpHost");

            migrationBuilder.RenameColumn(
                name: "SmtpDetails_Email",
                table: "Schools",
                newName: "SmtpEmail");

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Schools",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "SmtpPassword",
                table: "Schools",
                newName: "SmtpDetails_Password");

            migrationBuilder.RenameColumn(
                name: "SmtpHost",
                table: "Schools",
                newName: "SmtpDetails_Host");

            migrationBuilder.RenameColumn(
                name: "SmtpEmail",
                table: "Schools",
                newName: "SmtpDetails_Email");

            migrationBuilder.AddColumn<int>(
                name: "SmtpDetails_Port",
                table: "Schools",
                type: "integer",
                nullable: true);
        }
    }
}
