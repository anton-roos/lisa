using System.Security.Claims;
using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    IUiEventService uiEventService,
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    ProtectedSessionStorage sessionStorage,
    ILogger<SchoolService> logger
)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly IUiEventService _uiEventService = uiEventService;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ProtectedSessionStorage _sessionStorage = sessionStorage;
    private readonly ILogger<SchoolService> _logger = logger;
    private School? _selectedSchool;

    /// <summary>
    /// Sets the current selected school.
    /// </summary>
    public async Task<School?> SetCurrentSchoolAsync(Guid? schoolId)
    {
        try
        {
            if (schoolId == null)
            {
                _selectedSchool = null;
                await _sessionStorage.SetAsync("selectedSchool", string.Empty);
                await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
                return null;
            }

            using var context = await _dbContextFactory.CreateDbContextAsync();
            _selectedSchool = await context.Schools.FindAsync(schoolId);

            if (_selectedSchool == null)
            {
                _logger.LogWarning("Attempted to select a school that does not exist. SchoolId: {SchoolId}", schoolId);
                return null;
            }

            await _sessionStorage.SetAsync("selectedSchool", _selectedSchool);
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
    /// Gets the currently logged-in user.
    /// </summary>
    public async Task<IdentityUser<Guid>?> GetCurrentUserAsync()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? await _userManager.FindByIdAsync(userId) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user.");
            return null;
        }
    }

    public School? GetSelectedSchool() => _selectedSchool;

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
