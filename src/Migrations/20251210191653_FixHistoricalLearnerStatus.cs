using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class FixHistoricalLearnerStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix historical data from the ConvertLearnerActiveToStatus migration (20251210063407)
            // That migration incorrectly mapped:
            //   Active = true  -> Status = 0 (which was supposed to be "Active" but is now "Initial" in the enum)
            //   Active = false -> Status = 1 (which was supposed to be "Inactive" but is now "Active" in the enum)
            //
            // Current LearnerStatus enum values:
            //   Initial = 0, Active = 1, PendingPromotion = 2, Promoted = 3, 
            //   Retained = 4, Transferred = 5, Graduated = 6, Disabled = 7
            //
            // We need to correct:
            //   Status = 0 (Initial, but should be Active) -> Status = 1 (Active)
            //   Status = 1 (Active, but should be Disabled) -> Status = 7 (Disabled)
            //
            // We do this in a specific order to avoid conflicts:
            // 1. First update Status 0 to a temporary value (-1)
            // 2. Then update Status 1 to 7 (Disabled)
            // 3. Finally update Status -1 to 1 (Active)

            migrationBuilder.Sql(@"
                -- Step 1: Move Status 0 to temporary value -1
                UPDATE ""Learners"" 
                SET ""Status"" = -1 
                WHERE ""Status"" = 0;
            ");

            migrationBuilder.Sql(@"
                -- Step 2: Update Status 1 to 7 (Disabled - these were previously inactive learners)
                UPDATE ""Learners"" 
                SET ""Status"" = 7 
                WHERE ""Status"" = 1;
            ");

            migrationBuilder.Sql(@"
                -- Step 3: Update Status -1 to 1 (Active - these were previously active learners)
                UPDATE ""Learners"" 
                SET ""Status"" = 1 
                WHERE ""Status"" = -1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the migration:
            //   Status = 1 (Active) -> Status = 0
            //   Status = 7 (Disabled) -> Status = 1
            
            migrationBuilder.Sql(@"
                -- Step 1: Move Status 1 to temporary value -1
                UPDATE ""Learners"" 
                SET ""Status"" = -1 
                WHERE ""Status"" = 1;
            ");

            migrationBuilder.Sql(@"
                -- Step 2: Update Status 7 to 1
                UPDATE ""Learners"" 
                SET ""Status"" = 1 
                WHERE ""Status"" = 7;
            ");

            migrationBuilder.Sql(@"
                -- Step 3: Update Status -1 to 0
                UPDATE ""Learners"" 
                SET ""Status"" = 0 
                WHERE ""Status"" = -1;
            ");
        }
    }
}
