using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate38 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DietaryRequirements",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalAidName",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalAidNumber",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalAidPlan",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalAilments",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalInstructions",
                table: "Learners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicalTransport",
                table: "Learners",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "DietaryRequirements",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MedicalAidName",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MedicalAidNumber",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MedicalAidPlan",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MedicalAilments",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MedicalInstructions",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MedicalTransport",
                table: "Learners");
        }
    }
}
