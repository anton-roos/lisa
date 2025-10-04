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
}