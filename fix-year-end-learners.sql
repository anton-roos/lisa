-- PostgreSQL Script to Fix Year-End Learner Issues
-- This script addresses bugs in the year-end promotion workflow

-- REPLACE THIS WITH YOUR SCHOOL'S ID
\set school_id '00000000-0000-0000-0000-000000000000'

-- ==============================================================================
-- DIAGNOSIS: Check current state of learners
-- ==============================================================================

\echo '========================================='
\echo 'CURRENT LEARNER STATUS SUMMARY'
\echo '========================================='

SELECT 
    "Status",
    COUNT(*) as count,
    STRING_AGG(DISTINCT "Surname" || ', ' || "Name", '; ' ORDER BY "Surname" || ', ' || "Name") as sample_learners
FROM "Learners"
WHERE "SchoolId" = :'school_id'
GROUP BY "Status"
ORDER BY "Status";

\echo ''
\echo '========================================='
\echo 'LEARNERS STUCK IN PROMOTED/RETAINED STATUS'
\echo '========================================='

SELECT 
    "Id",
    "Code",
    "Surname",
    "Name",
    "Status",
    "RegisterClassId",
    "PreviousRegisterClassId",
    "PreviousSchoolGradeId"
FROM "Learners"
WHERE "SchoolId" = :'school_id'
  AND "Status" IN (3, 4) -- 3=Promoted, 4=Retained
ORDER BY "Surname", "Name";

\echo ''
\echo '========================================='
\echo 'LEARNERS WITH MISSING PREVIOUS GRADE INFO'
\echo '========================================='

SELECT 
    "Id",
    "Code",
    "Surname",
    "Name",
    "Status",
    "RegisterClassId",
    "PreviousRegisterClassId" IS NULL as missing_prev_class,
    "PreviousSchoolGradeId" IS NULL as missing_prev_grade
FROM "Learners"
WHERE "SchoolId" = :'school_id'
  AND "Status" IN (2, 3, 4) -- PendingPromotion, Promoted, Retained
  AND ("PreviousRegisterClassId" IS NULL OR "PreviousSchoolGradeId" IS NULL)
ORDER BY "Surname", "Name";

-- ==============================================================================
-- FIX #1: Restore Promoted/Retained learners to Active status
-- ==============================================================================

\echo ''
\echo '========================================='
\echo 'FIX #1: Setting Promoted/Retained learners to Active'
\echo '========================================='

BEGIN;

UPDATE "Learners"
SET "Status" = 1, -- Active
    "UpdatedAt" = NOW()
WHERE "SchoolId" = :'school_id'
  AND "Status" IN (3, 4); -- Promoted, Retained

\echo 'Updated ' || (SELECT COUNT(*) FROM "Learners" WHERE "SchoolId" = :'school_id' AND "Status" IN (3, 4)) || ' learner(s)'

-- Uncomment to apply changes:
-- COMMIT;
ROLLBACK; -- Remove this line and uncomment COMMIT above to apply

-- ==============================================================================
-- FIX #2: Set PreviousSchoolGradeId and PreviousRegisterClassId if missing
-- ==============================================================================

\echo ''
\echo '========================================='
\echo 'FIX #2: Setting missing Previous grade/class information'
\echo '========================================='

BEGIN;

-- For learners who have a current RegisterClass but missing Previous info
UPDATE "Learners" l
SET 
    "PreviousRegisterClassId" = l."RegisterClassId",
    "PreviousSchoolGradeId" = rc."SchoolGradeId",
    "UpdatedAt" = NOW()
FROM "RegisterClasses" rc
WHERE l."RegisterClassId" = rc."Id"
  AND l."SchoolId" = :'school_id'
  AND l."Status" IN (2, 3, 4) -- PendingPromotion, Promoted, Retained
  AND (l."PreviousRegisterClassId" IS NULL OR l."PreviousSchoolGradeId" IS NULL);

\echo 'Updated ' || (SELECT COUNT(*) FROM "Learners" WHERE "SchoolId" = :'school_id' AND "Status" IN (2, 3, 4) AND ("PreviousRegisterClassId" IS NULL OR "PreviousSchoolGradeId" IS NULL)) || ' learner(s)'

-- Uncomment to apply changes:
-- COMMIT;
ROLLBACK; -- Remove this line and uncomment COMMIT above to apply

-- ==============================================================================
-- FIX #3: Clear RegisterClassId for Promoted learners (if needed)
-- ==============================================================================

\echo ''
\echo '========================================='
\echo 'FIX #3: Clearing RegisterClassId for Promoted learners'
\echo '========================================='
\echo 'Note: This is optional and only needed if promoted learners'
\echo '      should not be linked to their old register classes'
\echo ''

BEGIN;

UPDATE "Learners"
SET 
    "RegisterClassId" = NULL,
    "UpdatedAt" = NOW()
WHERE "SchoolId" = :'school_id'
  AND "Status" = 3 -- Promoted
  AND "RegisterClassId" IS NOT NULL
  AND "PreviousRegisterClassId" IS NOT NULL; -- Only if we've preserved the previous one

\echo 'Would clear RegisterClassId for ' || (SELECT COUNT(*) FROM "Learners" WHERE "SchoolId" = :'school_id' AND "Status" = 3 AND "RegisterClassId" IS NOT NULL) || ' promoted learner(s)'

-- Uncomment to apply changes:
-- COMMIT;
ROLLBACK; -- Remove this line and uncomment COMMIT above to apply

-- ==============================================================================
-- FIX #4: Deactivate Year-End Mode if stuck
-- ==============================================================================

\echo ''
\echo '========================================='
\echo 'FIX #4: Check and deactivate Year-End Mode'
\echo '========================================='

SELECT 
    "Id",
    "Name",
    "IsYearEndMode",
    (SELECT COUNT(*) FROM "Learners" WHERE "SchoolId" = "Schools"."Id" AND "Status" = 2) as pending_count
FROM "Schools"
WHERE "Id" = :'school_id';

BEGIN;

-- Only deactivate if no learners are still pending
UPDATE "Schools"
SET "IsYearEndMode" = false
WHERE "Id" = :'school_id'
  AND "IsYearEndMode" = true
  AND NOT EXISTS (
    SELECT 1 FROM "Learners" 
    WHERE "SchoolId" = :'school_id' 
    AND "Status" = 2 -- PendingPromotion
  );

\echo 'Deactivated year-end mode: ' || (SELECT CASE WHEN "IsYearEndMode" THEN 'No (still active)' ELSE 'Yes' END FROM "Schools" WHERE "Id" = :'school_id')

-- Uncomment to apply changes:
-- COMMIT;
ROLLBACK; -- Remove this line and uncomment COMMIT above to apply

-- ==============================================================================
-- VERIFICATION: Check final state
-- ==============================================================================

\echo ''
\echo '========================================='
\echo 'FINAL LEARNER STATUS SUMMARY'
\echo '========================================='

SELECT 
    "Status",
    COUNT(*) as count,
    STRING_AGG(DISTINCT "Surname" || ', ' || "Name", '; ' ORDER BY "Surname" || ', ' || "Name") as sample_learners
FROM "Learners"
WHERE "SchoolId" = :'school_id'
GROUP BY "Status"
ORDER BY "Status";

\echo ''
\echo '========================================='
\echo 'INSTRUCTIONS:'
\echo '========================================='
\echo '1. Replace school_id at the top with your actual school GUID'
\echo '2. Review the diagnosis output above'
\echo '3. For each FIX section you want to apply:'
\echo '   - Comment out the ROLLBACK line'
\echo '   - Uncomment the COMMIT line'
\echo '4. Run the script again to apply changes'
\echo '========================================='
