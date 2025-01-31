using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class EmailReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LearnerId",
                table: "EmailRecipients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "EmailRecipients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "EmailRecipients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_LearnerId",
                table: "EmailRecipients",
                column: "LearnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_AspNetUsers_EmailCampaignId",
                table: "EmailRecipients",
                column: "EmailCampaignId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Parents_EmailCampaignId",
                table: "EmailRecipients",
                column: "EmailCampaignId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_AspNetUsers_EmailCampaignId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Parents_EmailCampaignId",
                table: "EmailRecipients");

            migrationBuilder.DropIndex(
                name: "IX_EmailRecipients_LearnerId",
                table: "EmailRecipients");

            migrationBuilder.DropColumn(
                name: "LearnerId",
                table: "EmailRecipients");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "EmailRecipients");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmailRecipients");
        }
    }
}
