using System.Security.Claims;
using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    UiEventService uiEventService,
    UserService userService,
    AuthenticationStateProvider authenticationStateProvider,
    ILogger<SchoolService> logger
)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly UiEventService _uiEventService = uiEventService;
    private readonly ILogger<SchoolService> _logger = logger;
    private readonly UserService _userService = userService;
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private School? _selectedSchool;

    /// <summary>
    /// Sets the current selected school and persists the selection in the user's record.
    /// For non-system administrators, the selected school must be valid.
    /// </summary>
    /// <param name="schoolId">The ID of the school to select, or null to clear the selection.</param>
    /// <returns>The selected <see cref="School"/> or null.</returns>
    public async Task<School?> SetCurrentSchoolAsync(Guid? schoolId)
    {
        try
        {
            if (schoolId == null)
            {
                _selectedSchool = null;
                await UpdateUserSelectedSchoolAsync();
                await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
                return null;
            }

            using var context = await _dbContextFactory.CreateDbContextAsync();
            var school = await context.Schools
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == schoolId);

            if (school == null)
            {
                _logger.LogWarning("Attempted to select a school that does not exist. SchoolId: {SchoolId}", schoolId);
                return null;
            }

            _selectedSchool = school;

            await UpdateUserSelectedSchoolAsync();

            await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
            return _selectedSchool;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current school. SchoolId: {SchoolId}", schoolId);
            return null;
        }
    }

    /// <summary>
    /// Retrieves the currently selected school.
    /// For non-system administrator users, a valid selected school is expected.
    /// </summary>
    /// <returns>The selected <see cref="School"/>, or null for system administrators.</returns>
    public async Task<School?> GetSelectedSchoolAsync()
    {
        if (_selectedSchool != null)
        {
            return _selectedSchool;
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            _logger.LogError("Unable to retrieve current user.");
            return null;
        }

        var user = await _userService.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            _logger.LogError("User data not found for current user with Id: {UserId}", currentUser.Id);
            return null;
        }

        if (user.Roles.Contains(Roles.SystemAdministrator))
        {
            _logger.LogError("Returning null for system administrator user {UserId}.", user.Id);
            return null;
        }

        if (user.SchoolId == null)
        {
            _logger.LogError("Non-system administrator user {UserId} does not have an associated selected school.", user.Id);
            throw new InvalidOperationException("Non-system administrator users must have an associated selected school.");
        }

        using var context = await _dbContextFactory.CreateDbContextAsync();
        _selectedSchool = await context.Schools
            .AsNoTracking().Include(school => school.Learners)
            .FirstOrDefaultAsync(s => s.Id == user.SchoolId);

        if (_selectedSchool == null)
        {
            _logger.LogError("School not found for non-system administrator user {UserId} with SchoolId: {SchoolId}", user.Id, user.SchoolId);
            throw new InvalidOperationException("Non-system administrator user must have a valid associated school.");
        }

        _logger.LogError("Main return returned school as {school} ", _selectedSchool.Learners);
        return _selectedSchool;
    }

    /// <summary>
    /// Retrieves the current user using the AuthenticationStateProvider.
    /// </summary>
    private async Task<IdentityUser<Guid>?> GetCurrentUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var userPrincipal = authState.User;

        if (userPrincipal.Identity is null || !userPrincipal.Identity.IsAuthenticated)
        {
            _logger.LogError("User is not authenticated.");
            return null;
        }

        var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("No user id claim found in the current authentication state.");
            return null;
        }

        try
        {
            return await _userService.GetByIdAsync(Guid.Parse(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user with Id: {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Updates the persistent store with the current selected school ID for the logged-in user.
    /// </summary>
    private async Task UpdateUserSelectedSchoolAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            _logger.LogError("Unable to update selected school because the current user is null.");
            return;
        }

        var user = await _userService.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            _logger.LogError("User data not found for the current user with Id: {UserId}", currentUser.Id);
            return;
        }

        await _userService.UpdateUserSelectedSchool(user);
    }


    /// <summary>
    /// Gets the total count of schools.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Schools.CountAsync();
    }

    /// <summary>
    /// Retrieves all schools.
    /// </summary>
    public async Task<List<School>> GetAllAsync()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Schools
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all schools.");
            return [];
        }
    }

    /// <summary>
    /// Retrieves all school types.
    /// </summary>
    public async Task<List<SchoolType>> GetSchoolTypesAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolTypes.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Retrieves a school by ID with related entities.
    /// </summary>
    public async Task<School?> GetSchoolAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
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
            _logger.LogError(ex, "Error fetching school with ID: {SchoolId}", id);
            return null;
        }
    }

    /// <summary>
    /// Retrieves all school curriculums.
    /// </summary>
    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolCurriculums.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new school.
    /// </summary>
    public async Task<bool> AddSchoolAsync(School school)
    {
        return await ModifySchoolAsync(async context =>
        {
            await context.Schools.AddAsync(school);
        });
    }

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    public async Task<bool> UpdateAsync(School school)
    {
        return await ModifySchoolAsync(context =>
        {
            context.Schools.Update(school);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Deletes a school.
    /// </summary>
    public async Task<bool> DeleteSchoolAsync(School school)
    {
        return await ModifySchoolAsync(context =>
        {
            context.Schools.Remove(school);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Modifies a school entity and triggers an event.
    /// </summary>
    private async Task<bool> ModifySchoolAsync(Func<LisaDbContext, Task> action)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            await action(context);
            await context.SaveChangesAsync();
            await _uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying school data.");
            return false;
        }
    }
}
