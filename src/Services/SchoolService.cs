using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Lisa.Services;

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

    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SchoolCurriculums.AsNoTracking().ToListAsync();
    }

    public async Task<bool> AddSchoolAsync(School school)
    {
        return await ModifySchoolAsync(async context =>
        {
            await context.Schools.AddAsync(school);
        });
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
        return await ModifySchoolAsync(context =>
        {
            context.Schools.Remove(school);
            return Task.CompletedTask;
        });
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

    public async Task<bool> ActivateYearEndModeAsync(Guid schoolId)
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

            int academicYear = DateTime.UtcNow.Year;
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
                    Year = academicYear,
                    SchoolGradeId = learner.RegisterClass?.SchoolGradeId ?? learner.Combination?.SchoolGradeId ?? Guid.Empty,
                    RegisterClassId = learner.RegisterClassId,
                    CombinationId = learner.CombinationId,
                    SubjectSnapshot = subjectSnapshot != null ? System.Text.Json.JsonSerializer.Serialize(subjectSnapshot) : "[]",
                    Outcome = Lisa.Enums.PromotionStatus.PromotionPending,
                    CreatedAt = DateTime.UtcNow
                };
                
                context.LearnerAcademicRecords.Add(historyRecord);

                // Archive the learner's state
                learner.Status = Enums.LearnerStatus.PendingPromotion;
                
                // Remove all subjects (now archived in academic record)
                if (learner.LearnerSubjects != null && learner.LearnerSubjects.Any())
                {
                    subjectsArchived += learner.LearnerSubjects.Count;
                    context.Set<LearnerSubject>().RemoveRange(learner.LearnerSubjects);
                }
                
                // Archive all results (mark with academic year, don't delete)
                if (learner.Results != null && learner.Results.Any())
                {
                    resultsArchived += learner.Results.Count;
                    foreach (var result in learner.Results)
                    {
                        result.AcademicYear = academicYear;
                    }
                }
            }
            
            // Archive all combinations for this school
            var combinations = await context.Combinations
                .Where(c => c.SchoolGrade!.SchoolId == schoolId && !c.IsArchived)
                .ToListAsync();
                
            foreach (var combination in combinations)
            {
                combination.IsArchived = true;
                combination.AcademicYear = academicYear;
            }
            
            // Archive all result sets for this school
            var resultSets = await context.ResultSets
                .Include(rs => rs.SchoolGrade)
                .Where(rs => rs.SchoolGrade!.SchoolId == schoolId && !rs.IsArchived)
                .ToListAsync();
                
            foreach (var resultSet in resultSets)
            {
                resultSet.IsArchived = true;
                resultSet.AcademicYear = academicYear;
            }

            await context.SaveChangesAsync();
            await uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);

            logger.LogInformation("Year-end mode activated for school {SchoolId}. {LearnerCount} learners archived. {SubjectsArchived} subjects removed, {ResultsArchived} results archived, {CombinationCount} combinations archived, {ResultSetCount} result sets archived.", 
                schoolId, learners.Count, subjectsArchived, resultsArchived, combinations.Count, resultSets.Count);

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

            // Restore learners from PendingPromotion state
            // Only restore learners who haven't been processed (still pending)
            // Promoted/Retained learners should have already been moved to their new grades
            var archivedLearners = await context.Learners
                .Where(l => l.SchoolId == schoolId && 
                           l.Status == Enums.LearnerStatus.PendingPromotion)
                .ToListAsync();

            foreach (var learner in archivedLearners)
            {
                // Restore learners who weren't processed back to Active
                learner.Status = Enums.LearnerStatus.Active;
            }

            await context.SaveChangesAsync();
            await uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);

            logger.LogInformation("Year-end mode deactivated for school {SchoolId}. {LearnerCount} unprocessed learners restored to Active status.", 
                schoolId, archivedLearners.Count);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating year-end mode for school {SchoolId}", schoolId);
            return false;
        }
    }
}