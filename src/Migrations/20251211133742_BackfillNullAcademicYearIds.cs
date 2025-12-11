using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class BackfillNullAcademicYearIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create academic years for schools that don't have any
            // This creates a 2025 academic year (current) for each school without one
            migrationBuilder.Sql(@"
                INSERT INTO ""AcademicYears"" (""Id"", ""SchoolId"", ""Year"", ""IsCurrent"", ""CreatedAt"")
                SELECT 
                    gen_random_uuid(),
                    s.""Id"",
                    2025,
                    true,
                    NOW()
                FROM ""Schools"" s
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""AcademicYears"" ay WHERE ay.""SchoolId"" = s.""Id""
                );
            ");

            // Step 2: Ensure exactly one academic year is current per school
            // If a school has academic years but none is current, set the latest one as current
            migrationBuilder.Sql(@"
                UPDATE ""AcademicYears"" ay
                SET ""IsCurrent"" = true
                WHERE ay.""Id"" IN (
                    SELECT DISTINCT ON (""SchoolId"") ""Id""
                    FROM ""AcademicYears""
                    WHERE ""SchoolId"" IN (
                        SELECT ""SchoolId"" 
                        FROM ""AcademicYears"" 
                        GROUP BY ""SchoolId"" 
                        HAVING SUM(CASE WHEN ""IsCurrent"" THEN 1 ELSE 0 END) = 0
                    )
                    ORDER BY ""SchoolId"", ""Year"" DESC
                );
            ");

            // Step 3: Backfill RegisterClasses - get school from SchoolGrade
            migrationBuilder.Sql(@"
                UPDATE ""RegisterClasses"" rc
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""SchoolGrades"" sg ON sg.""SchoolId"" = ay.""SchoolId""
                    WHERE sg.""Id"" = rc.""SchoolGradeId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE rc.""AcademicYearId"" IS NULL;
            ");

            // Step 4: Backfill Combinations - get school from SchoolGrade
            migrationBuilder.Sql(@"
                UPDATE ""Combinations"" c
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""SchoolGrades"" sg ON sg.""SchoolId"" = ay.""SchoolId""
                    WHERE sg.""Id"" = c.""SchoolGradeId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE c.""AcademicYearId"" IS NULL;
            ");

            // Step 5: Backfill ResultSets - get school from SchoolGrade
            migrationBuilder.Sql(@"
                UPDATE ""ResultSets"" rs
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""SchoolGrades"" sg ON sg.""SchoolId"" = ay.""SchoolId""
                    WHERE sg.""Id"" = rs.""SchoolGradeId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE rs.""AcademicYearId"" IS NULL AND rs.""SchoolGradeId"" IS NOT NULL;
            ");

            // Step 6: Backfill Results - get from Learner's school
            migrationBuilder.Sql(@"
                UPDATE ""Results"" r
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                    WHERE l.""Id"" = r.""LearnerId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE r.""AcademicYearId"" IS NULL;
            ");

            // Step 7: Backfill LearnerSubjects - get from Learner's school
            migrationBuilder.Sql(@"
                UPDATE ""LearnerSubjects"" ls
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                    WHERE l.""Id"" = ls.""LearnerId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE ls.""AcademicYearId"" IS NULL;
            ");

            // Step 8: Backfill Attendances - get from School directly
            migrationBuilder.Sql(@"
                UPDATE ""Attendances"" a
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    WHERE ay.""SchoolId"" = a.""SchoolId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE a.""AcademicYearId"" IS NULL AND a.""SchoolId"" IS NOT NULL;
            ");

            // Step 9: Backfill LeaveEarlies - get from Learner's school
            migrationBuilder.Sql(@"
                UPDATE ""LeaveEarlies"" le
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                    WHERE l.""Id"" = le.""LearnerId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE le.""AcademicYearId"" IS NULL;
            ");

            // Step 10: Backfill AcademicDevelopmentClasses - get from School directly
            migrationBuilder.Sql(@"
                UPDATE ""AcademicDevelopmentClasses"" adc
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    WHERE ay.""SchoolId"" = adc.""SchoolId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE adc.""AcademicYearId"" IS NULL;
            ");

            // Step 11: Backfill LearnerAcademicRecords (these should already have AcademicYearId from year-end process)
            // But just in case, backfill any that are null
            migrationBuilder.Sql(@"
                UPDATE ""LearnerAcademicRecords"" lar
                SET ""AcademicYearId"" = (
                    SELECT ay.""Id"" 
                    FROM ""AcademicYears"" ay
                    JOIN ""Learners"" l ON l.""SchoolId"" = ay.""SchoolId""
                    WHERE l.""Id"" = lar.""LearnerId"" AND ay.""IsCurrent"" = true
                    LIMIT 1
                )
                WHERE lar.""AcademicYearId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: This migration is a data backfill and cannot be fully reversed
            // Setting AcademicYearId back to NULL would break the application
            // If you need to rollback, restore from a database backup
        }
    }
}
