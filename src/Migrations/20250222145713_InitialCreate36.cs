using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate36 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "ContentHtml",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "EmailTemplate",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "SenderEmail",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "StatsClickCount",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "StatsOpenCount",
                table: "EmailCampaigns");

            migrationBuilder.RenameColumn(
                name: "StatsSentCount",
                table: "EmailCampaigns",
                newName: "RecipientTemplate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipientTemplate",
                table: "EmailCampaigns",
                newName: "StatsSentCount");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "EmailCampaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentHtml",
                table: "EmailCampaigns",
                type: "character varying(8912)",
                maxLength: 8912,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailTemplate",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "EmailCampaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "EmailCampaigns",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "EmailCampaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatsClickCount",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatsOpenCount",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
