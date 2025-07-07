using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate34 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailCampaigns_EmailTemplates_TemplateId",
                table: "EmailCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_EmailCampaigns_TemplateId",
                table: "EmailCampaigns");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "EmailCampaigns");

            migrationBuilder.AddColumn<int>(
                name: "EmailTemplate",
                table: "EmailCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTemplate",
                table: "EmailCampaigns");

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "EmailCampaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EmailCampaigns_TemplateId",
                table: "EmailCampaigns",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailCampaigns_EmailTemplates_TemplateId",
                table: "EmailCampaigns",
                column: "TemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
