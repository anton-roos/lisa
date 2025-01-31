using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class CampaignDate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_ParentId",
                table: "EmailRecipients",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_UserId",
                table: "EmailRecipients",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_AspNetUsers_UserId",
                table: "EmailRecipients",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Parents_ParentId",
                table: "EmailRecipients",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_AspNetUsers_UserId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Learners_LearnerId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Parents_ParentId",
                table: "EmailRecipients");

            migrationBuilder.DropIndex(
                name: "IX_EmailRecipients_ParentId",
                table: "EmailRecipients");

            migrationBuilder.DropIndex(
                name: "IX_EmailRecipients_UserId",
                table: "EmailRecipients");

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
    }
}
