using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Lisa.Services;

public record SchoolDeleteCounts(int StaffCount, int LearnerCount, int RegisterClassCount);

public class SchoolService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    UiEventService uiEventService,
    UserService userService,
    AuthenticationStateProvider authenticationStateProvider,
    ILogger<SchoolService> logger
)
{
    private School? _selectedSchool;

    public async Task<School?> SetCurrentSchoolAsync(Guid? schoolId)
    {
        try
        {
            if (schoolId == null)
            {
                _selectedSchool = null;
                await UpdateUserSelectedSchoolAsync();
                await uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
                return null;
            }

            await using var context = await dbContextFactory.CreateDbContextAsync();
            var school = await context.Schools
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == schoolId);

            if (school == null)
            {
                logger.LogWarning("Attempted to select a school that does not exist. SchoolId: {SchoolId}", schoolId);
                return null;
            }

            _selectedSchool = school;

            await UpdateUserSelectedSchoolAsync();

            await uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
            return _selectedSchool;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting current school. SchoolId: {SchoolId}", schoolId);
            return null;
        }
    }

    public async Task<School?> GetCurrentSchoolAsync()
    {
        if (_selectedSchool != null)
        {
            return _selectedSchool;
        }
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            logger.LogDebug("Unable to retrieve current user - user may not be authenticated yet.");
            return null;
        }

        var user = await userService.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            logger.LogError("User data not found for current user with Id: {UserId}", currentUser.Id);
            return null;
        }

        if (user.Roles.Contains(Roles.SystemAdministrator))
        {
            logger.LogDebug("System administrator user {UserId} does not have a specific school context - this is expected behavior.", user.Id);
            return null;
        }

        if (user.SchoolId == null)
        {
            logger.LogError("Non-system administrator user {UserId} does not have an associated selected school.", user.Id);
            throw new InvalidOperationException("Non-system administrator users must have an associated selected school.");
        }

        await using var context = await dbContextFactory.CreateDbContextAsync();
        _selectedSchool = await context.Schools
            .AsNoTracking().Include(school => school.Learners)
            .FirstOrDefaultAsync(s => s.Id == user.SchoolId);
        
        return _selectedSchool;
    }

    // Backward compatibility methods
    public async Task<School?> GetSelectedSchoolAsync() => await GetCurrentSchoolAsync();
    public async Task<List<School>> GetAllAsync() => await GetAllSchoolsAsync();

    public async Task<List<AcademicYear>> GetAcademicYearsForSchoolAsync(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AcademicYears
            .AsNoTracking()
            .Where(ay => ay.SchoolId == schoolId)
            .OrderByDescending(ay => ay.Year)
            .ToListAsync();
    }

    public async Task<AcademicYear?> GetCurrentAcademicYearAsync(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(ay => ay.SchoolId == schoolId && ay.IsCurrent);
    }

    public async Task<Guid?> GetCurrentAcademicYearIdAsync(Guid schoolId)
    {
        var academicYear = await GetCurrentAcademicYearAsync(schoolId);
        return academicYear?.Id;
    }

    public async Task<AcademicYear?> AddAcademicYearAsync(Guid schoolId, int year)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        // Check if academic year already exists for this school
        var existing = await context.AcademicYears
            .FirstOrDefaultAsync(ay => ay.SchoolId == schoolId && ay.Year == year);
        
        if (existing != null)
        {
            return null; // Already exists
        }

        // Check if this is the first academic year for the school - if so, set as current
        var hasAnyAcademicYear = await context.AcademicYears
            .AnyAsync(ay => ay.SchoolId == schoolId);

        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            Year = year,
            IsCurrent = !hasAnyAcademicYear, // First year becomes current automatically
            CreatedAt = DateTime.UtcNow
        };

        context.AcademicYears.Add(academicYear);
        await context.SaveChangesAsync();
        
        if (!hasAnyAcademicYear)
        {
            logger.LogInformation("Created first academic year {Year} for school {SchoolId} - set as current", year, schoolId);
        }
        
        return academicYear;
    }

    public async Task<bool> SetCurrentAcademicYearAsync(Guid schoolId, Guid academicYearId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        // Verify the target academic year exists and belongs to this school
        var targetYear = await context.AcademicYears
            .FirstOrDefaultAsync(ay => ay.Id == academicYearId && ay.SchoolId == schoolId);
        
        if (targetYear == null)
        {
            logger.LogWarning("Academic year {AcademicYearId} not found for school {SchoolId}", academicYearId, schoolId);
            return false;
        }

        // Set all academic years for this school: only the target becomes current
        var allYears = await context.AcademicYears
            .Where(ay => ay.SchoolId == schoolId)
            .ToListAsync();
        
        foreach (var year in allYears)
        {
            year.IsCurrent = year.Id == academicYearId;
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Set academic year {Year} as current for school {SchoolId}", targetYear.Year, schoolId);
        return true;
    }

    private async Task<IdentityUser<Guid>?> GetCurrentUserAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var userPrincipal = authState.User;

        if (userPrincipal.Identity is null || !userPrincipal.Identity.IsAuthenticated)
        {
            logger.LogDebug("User is not authenticated - this is normal during startup or for anonymous requests.");
            return null;
        }

        var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("No user id claim found in the current authentication state.");
            return null;
        }

        try
        {
            return await userService.GetByIdAsync(Guid.Parse(userId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving current user with Id: {UserId}", userId);
            return null;
        }
    }

    private async Task UpdateUserSelectedSchoolAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            logger.LogError("Unable to update selected school because the current user is null.");
            return;
        }

        var user = await userService.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            logger.LogError("User data not found for the current user with Id: {UserId}", currentUser.Id);
            return;
        }

        await userService.UpdateUserSelectedSchool(user);
    }

    public async Task<int> GetCountAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Schools.CountAsync();
    }

    public async Task<List<School>> GetAllSchoolsAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Schools
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all schools.");
            return [];
        }
    }

    public async Task<List<SchoolType>> GetSchoolTypesAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SchoolTypes.AsNoTracking().ToListAsync();
    }

    public async Task<School?> GetSchoolAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Schools
                .AsNoTracking()
                .Include(s => s.SchoolType!)
                .Include(s => s.Curriculum!)
                .Include(s => s.SchoolGrades!)
                .ThenInclude(sg => sg.SystemGrade)
                .Include(s => s.RegisterClasses!)
                .Include(s => s.Staff!)
                .Include(s => s.Learners!)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching school with ID: {SchoolId}", id);
            return null;
        }
    }

    public async Task<SchoolDeleteCounts?> GetSchoolWithCountsAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            var staffCount = await context.Users.CountAsync(u => u.SchoolId == schoolId);
            var learnerCount = await context.Learners.CountAsync(l => l.SchoolId == schoolId);
            
            var schoolGradeIds = await context.SchoolGrades
                .Where(sg => sg.SchoolId == schoolId)
                .Select(sg => sg.Id)
                .ToListAsync();
            
            var registerClassCount = await context.RegisterClasses
                .CountAsync(rc => schoolGradeIds.Contains(rc.SchoolGradeId));
            
            return new SchoolDeleteCounts(staffCount, learnerCount, registerClassCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching school counts for ID: {SchoolId}", schoolId);
            return null;
        }
    }

    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SchoolCurriculums.AsNoTracking().ToListAsync();
    }

    public async Task<bool> AddSchoolAsync(School school)
    {
        var success = await ModifySchoolAsync(async context =>
        {
            await context.Schools.AddAsync(school);
        });

        if (success)
        {
            // Automatically create the first academic year for the new school
            var currentYear = DateTime.UtcNow.Year;
            await AddAcademicYearAsync(school.Id, currentYear);
            logger.LogInformation("Created initial academic year {Year} for new school {SchoolId}", currentYear, school.Id);
        }

        return success;
    }

    public async Task<bool> UpdateAsync(School school)
    {
        return await ModifySchoolAsync(context =>
        {
            context.Schools.Update(school);
            return Task.CompletedTask;
        });
    }

    public async Task<bool> DeleteSchoolAsync(School school)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            var schoolId = school.Id;
            
            // Get AcademicYear IDs for this school (needed for AcademicEntity queries)
            var academicYearIds = await context.AcademicYears
                .Where(ay => ay.SchoolId == schoolId)
                .Select(ay => ay.Id)
                .ToListAsync();
            
            // Get SchoolGrade IDs for this school
            var schoolGradeIds = await context.SchoolGrades
                .Where(sg => sg.SchoolId == schoolId)
                .Select(sg => sg.Id)
                .ToListAsync();
            
            // Get learner IDs for this school
            var learnerIds = await context.Learners
                .Where(l => l.SchoolId == schoolId)
                .Select(l => l.Id)
                .ToListAsync();
            
            // Get user IDs for this school
            var userIds = await context.Users
                .Where(u => u.SchoolId == schoolId)
                .Select(u => u.Id)
                .ToListAsync();
            
            // Delete in order respecting foreign key constraints
            // Use ExecuteDeleteAsync for direct SQL execution to ensure proper ordering
            // Use IgnoreQueryFilters() to include soft-deleted records
            
            // BATCH 1: Delete all AcademicEntity types BEFORE AcademicYears
            
            // 1. Delete Results (references ResultSets and Learners)
            await context.Results
                .IgnoreQueryFilters()
                .Where(r => learnerIds.Contains(r.LearnerId))
                .ExecuteDeleteAsync();
            
            // 2. Delete ResultSets (AcademicEntity - references AcademicYears, Users via CapturedById/TeacherId)
            await context.ResultSets
                .IgnoreQueryFilters()
                .Where(rs => 
                    (rs.AcademicYearId.HasValue && academicYearIds.Contains(rs.AcademicYearId.Value)) ||
                    userIds.Contains(rs.CapturedById) ||
                    (rs.TeacherId.HasValue && userIds.Contains(rs.TeacherId.Value)) ||
                    (rs.SchoolGradeId.HasValue && schoolGradeIds.Contains(rs.SchoolGradeId.Value)))
                .ExecuteDeleteAsync();
            
            // 3. Delete AcademicDevelopmentClasses (AcademicEntity)
            await context.AcademicDevelopmentClasses
                .IgnoreQueryFilters()
                .Where(adc => adc.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 4. Delete AttendanceRecords first (references Attendances)
            await context.AttendanceRecords
                .IgnoreQueryFilters()
                .Where(ar => ar.Attendance != null && ar.Attendance.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 5. Delete Attendances (AcademicEntity)
            await context.Attendances
                .IgnoreQueryFilters()
                .Where(a => a.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 6. Delete LearnerSubjects (AcademicEntity)
            await context.LearnerSubjects
                .IgnoreQueryFilters()
                .Where(ls => learnerIds.Contains(ls.LearnerId))
                .ExecuteDeleteAsync();
            
            // 7. Delete LearnerAcademicRecords (AcademicEntity)
            await context.LearnerAcademicRecords
                .IgnoreQueryFilters()
                .Where(lar => learnerIds.Contains(lar.LearnerId))
                .ExecuteDeleteAsync();
            
            // 8. Delete LeaveEarlies (AcademicEntity)
            await context.LeaveEarlies
                .IgnoreQueryFilters()
                .Where(le => le.LearnerId.HasValue && learnerIds.Contains(le.LearnerId.Value))
                .ExecuteDeleteAsync();
            
            // 9. Delete RegisterClasses (AcademicEntity - references SchoolGrades and AcademicYears)
            // Use IgnoreQueryFilters to include soft-deleted records
            await context.RegisterClasses
                .IgnoreQueryFilters()
                .Where(rc => schoolGradeIds.Contains(rc.SchoolGradeId) ||
                            (rc.AcademicYearId.HasValue && academicYearIds.Contains(rc.AcademicYearId.Value)))
                .ExecuteDeleteAsync();
            
            // 10. Delete Combinations (AcademicEntity - references SchoolGrades and AcademicYears)
            // Delete by both SchoolGradeId AND AcademicYearId to catch all references
            // Use IgnoreQueryFilters to include soft-deleted records
            await context.Combinations
                .IgnoreQueryFilters()
                .Where(c => schoolGradeIds.Contains(c.SchoolGradeId) || 
                           (c.AcademicYearId.HasValue && academicYearIds.Contains(c.AcademicYearId.Value)))
                .ExecuteDeleteAsync();
            
            // BATCH 2: Now safe to delete AcademicYears
            
            // 11. Delete AcademicYears
            await context.AcademicYears
                .IgnoreQueryFilters()
                .Where(ay => ay.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // BATCH 3: Delete other school-related entities
            
            // 12. Delete EmailCampaigns
            await context.EmailCampaigns
                .IgnoreQueryFilters()
                .Where(ec => ec.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 13. Delete Periods
            await context.Periods
                .IgnoreQueryFilters()
                .Where(p => p.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 14. Delete CareGroups
            await context.CareGroups
                .IgnoreQueryFilters()
                .Where(cg => cg.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 15. Delete EmailRecipients (references Parents, Learners, Users)
            await context.EmailRecipients
                .IgnoreQueryFilters()
                .Where(er => 
                    (er.LearnerId.HasValue && learnerIds.Contains(er.LearnerId.Value)) ||
                    (er.UserId.HasValue && userIds.Contains(er.UserId.Value)))
                .ExecuteDeleteAsync();
            
            // 16. Delete Parents (references Learners)
            await context.Parents
                .IgnoreQueryFilters()
                .Where(p => learnerIds.Contains(p.LearnerId))
                .ExecuteDeleteAsync();
            
            // 17. Delete Learners
            await context.Learners
                .IgnoreQueryFilters()
                .Where(l => l.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 18. Delete Users (Staff)
            await context.Users
                .IgnoreQueryFilters()
                .Where(u => u.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 19. Delete SchoolGrades
            await context.SchoolGrades
                .IgnoreQueryFilters()
                .Where(sg => sg.SchoolId == schoolId)
                .ExecuteDeleteAsync();
            
            // 20. Finally delete the School
            await context.Schools
                .IgnoreQueryFilters()
                .Where(s => s.Id == schoolId)
                .ExecuteDeleteAsync();
            
            await uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting school.");
            throw;
        }
    }

    private async Task<bool> ModifySchoolAsync(Func<LisaDbContext, Task> action)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            await action(context);
            await context.SaveChangesAsync();
            await uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error modifying school data.");
            return false;
        }
    }

    public async Task<bool> ActivateYearEndModeAsync(Guid schoolId, Guid newAcademicYearId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            var school = await context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
            if (school == null)
            {
                logger.LogWarning("School not found with ID: {SchoolId}", schoolId);
                return false;
            }

            // Prevent double activation
            if (school.IsYearEndMode)
            {
                logger.LogWarning("Year-end mode is already active for school {SchoolId}", schoolId);
                return false;
            }

            // Get the current academic year (the one being archived)
            var currentAcademicYear = await context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.SchoolId == schoolId && ay.IsCurrent);
            
            if (currentAcademicYear == null)
            {
                logger.LogWarning("No current academic year found for school {SchoolId}", schoolId);
                return false;
            }

            // Get the new academic year that will become current
            var newAcademicYear = await context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Id == newAcademicYearId && ay.SchoolId == schoolId);
            
            if (newAcademicYear == null)
            {
                logger.LogWarning("New academic year {AcademicYearId} not found for school {SchoolId}", newAcademicYearId, schoolId);
                return false;
            }

            if (newAcademicYear.Id == currentAcademicYear.Id)
            {
                logger.LogWarning("Cannot transition to the same academic year");
                return false;
            }

            school.IsYearEndMode = true;
            context.Schools.Update(school);

            // Get all active learners with their subjects, results, and related data
            var learners = await context.Learners
                .Include(l => l.LearnerSubjects!)
                    .ThenInclude(ls => ls.Subject)
                .Include(l => l.Results)
                .Include(l => l.RegisterClass)
                    .ThenInclude(rc => rc!.SchoolGrade)
                .Include(l => l.Combination)
                .Where(l => l.SchoolId == schoolId && l.Status == Enums.LearnerStatus.Active)
                .ToListAsync();

            int subjectsArchived = 0;
            int resultsArchived = 0;

            foreach (var learner in learners)
            {
                // Create academic record to archive current state
                var subjectSnapshot = learner.LearnerSubjects?
                    .Select(ls => new { ls.SubjectId, ls.Subject?.Name, ls.Subject?.Code })
                    .ToList();

                var historyRecord = new LearnerAcademicRecord
                {
                    Id = Guid.NewGuid(),
                    LearnerId = learner.Id,
                    AcademicYearId = currentAcademicYear.Id,
                    SchoolGradeId = learner.RegisterClass?.SchoolGradeId ?? learner.Combination?.SchoolGradeId ?? Guid.Empty,
                    RegisterClassId = learner.RegisterClassId,
                    CombinationId = learner.CombinationId,
                    SubjectSnapshot = subjectSnapshot != null ? System.Text.Json.JsonSerializer.Serialize(subjectSnapshot) : "[]",
                    Outcome = Lisa.Enums.PromotionStatus.PromotionPending,
                    CreatedAt = DateTime.UtcNow
                };
                
                context.LearnerAcademicRecords.Add(historyRecord);

                // Archive the learner's state and preserve their grade/class info
                learner.Status = Enums.LearnerStatus.PendingPromotion;
                learner.PreviousSchoolGradeId = learner.RegisterClass?.SchoolGradeId;
                learner.PreviousRegisterClassId = learner.RegisterClassId;
                
                // Remove all subjects (now archived in academic record)
                if (learner.LearnerSubjects != null && learner.LearnerSubjects.Any())
                {
                    subjectsArchived += learner.LearnerSubjects.Count;
                    context.Set<LearnerSubject>().RemoveRange(learner.LearnerSubjects);
                }
                
                // Results are archived via their ResultSet's AcademicYearId
                if (learner.Results != null && learner.Results.Any())
                {
                    resultsArchived += learner.Results.Count;
                }
            }
            
            // Mark all combinations with the current academic year (archiving them)
            var combinations = await context.Combinations
                .Where(c => c.SchoolGrade!.SchoolId == schoolId && c.AcademicYearId == null)
                .ToListAsync();
                
            foreach (var combination in combinations)
            {
                combination.AcademicYearId = currentAcademicYear.Id;
            }
            
            // Mark all result sets with the current academic year (archiving them)
            var resultSets = await context.ResultSets
                .Include(rs => rs.SchoolGrade)
                .Where(rs => rs.SchoolGrade!.SchoolId == schoolId && rs.AcademicYearId == null)
                .ToListAsync();
                
            foreach (var resultSet in resultSets)
            {
                resultSet.AcademicYearId = currentAcademicYear.Id;
            }

            // Transition academic years: deactivate old, activate new
            currentAcademicYear.IsCurrent = false;
            newAcademicYear.IsCurrent = true;

            await context.SaveChangesAsync();
            await uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);

            logger.LogInformation("Year-end mode activated for school {SchoolId}. Transitioned from academic year {OldYear} to {NewYear}. {LearnerCount} learners archived. {SubjectsArchived} subjects removed, {ResultsArchived} results archived, {CombinationCount} combinations archived, {ResultSetCount} result sets archived.", 
                schoolId, currentAcademicYear.Year, newAcademicYear.Year, learners.Count, subjectsArchived, resultsArchived, combinations.Count, resultSets.Count);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating year-end mode for school {SchoolId}", schoolId);
            return false;
        }
    }

    public async Task<bool> DeactivateYearEndModeAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            var school = await context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
            if (school == null)
            {
                logger.LogWarning("School not found with ID: {SchoolId}", schoolId);
                return false;
            }

            // Check for learners still pending promotion decisions
            var pendingLearners = await context.Learners
                .Where(l => l.SchoolId == schoolId && 
                           l.Status == Enums.LearnerStatus.PendingPromotion)
                .ToListAsync();

            if (pendingLearners.Any())
            {
                logger.LogWarning("Cannot deactivate year-end mode for school {SchoolId}. {Count} learner(s) still have pending promotion decisions.", 
                    schoolId, pendingLearners.Count);
                return false;
            }

            school.IsYearEndMode = false;
            context.Schools.Update(school);

            // Restore promoted/retained learners to Active status
            var promotedRetainedLearners = await context.Learners
                .Where(l => l.SchoolId == schoolId && 
                           (l.Status == Enums.LearnerStatus.Promoted || 
                            l.Status == Enums.LearnerStatus.Retained))
                .ToListAsync();

            foreach (var learner in promotedRetainedLearners)
            {
                learner.Status = Enums.LearnerStatus.Active;
            }

            await context.SaveChangesAsync();
            await uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);

            logger.LogInformation("Year-end mode deactivated for school {SchoolId}. {ProcessedCount} promoted/retained learners restored to Active status.", 
                schoolId, promotedRetainedLearners.Count);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating year-end mode for school {SchoolId}", schoolId);
            return false;
        }
    }
}