using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class ConvertLearnerActiveToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new Status column with a default value
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Learners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Migrate existing data: Active = true -> Status = 0 (Active), Active = false -> Status = 1 (Inactive)
            migrationBuilder.Sql(@"
                UPDATE ""Learners"" 
                SET ""Status"" = CASE 
                    WHEN ""Active"" = true THEN 0 
                    ELSE 1 
                END
            ");

            // Drop the old Active column
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Learners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add the old Active column
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Learners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Migrate data back: Status = 0 (Active) -> Active = true, others -> Active = false
            migrationBuilder.Sql(@"
                UPDATE ""Learners"" 
                SET ""Active"" = CASE 
                    WHEN ""Status"" = 0 THEN true 
                    ELSE false 
                END
            ");

            // Drop the Status column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Learners");
        }
    }
}
