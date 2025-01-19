using System.Security.Claims;
using FluentAssertions;
using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;
    private readonly IUiEventService _uiEventService;
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProtectedSessionStorage _sessionStorage;

    private School? _selectedSchool;
    private static int _instanceCounter;
    private readonly int _instanceId;

    public SchoolService(
        IDbContextFactory<LisaDbContext> dbContextFactory,
        IUiEventService uiEventService,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        ProtectedSessionStorage sessionStorage
        )
    {
        _dbContextFactory = dbContextFactory;
        _uiEventService = uiEventService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _sessionStorage = sessionStorage;

        _instanceId = Interlocked.Increment(ref _instanceCounter);
        Console.WriteLine($"SchoolService created. ID: {_instanceId}");
    }

    public async Task<School> SetCurrentSchoolAsync(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        _selectedSchool = await context.Schools.FindAsync(schoolId);
        _selectedSchool.Should().NotBeNull();
        if (_selectedSchool != null)
        {
            await _sessionStorage.SetAsync("selectedSchool", _selectedSchool);
            await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
            return _selectedSchool;
        }
        else
        {
            throw new InvalidOperationException("School was null when trying to SetCurrentSchoolAsync");
        }
    }

    public async Task<IdentityUser<Guid>?> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId != null ? await _userManager.FindByIdAsync(userId) : null;
    }

    public School? GetSelectedSchool() => _selectedSchool;

    public async Task<int> GetCountAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Schools.CountAsync();
    }

    public async Task<List<School>> GetAllAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Schools.ToListAsync();
    }

    public async Task<List<SchoolType>> GetSchoolTypesAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolTypes.ToListAsync();
    }

    public async Task<School?> GetSchoolAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Schools
        .Include(s => s.SchoolType)
        .Include(s => s.Curriculum)
        .Where(x => x.Id == id)
        .FirstOrDefaultAsync();
    }

    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolCurriculums.ToListAsync();
    }

    public async Task AddSchoolAsync(School school)
    {
        await ModifySchoolAsync(async context => await Task.Run(() => context.Schools.Add(school)));
    }

    public async Task UpdateAsync(School school)
    {
        await ModifySchoolAsync(async context => await Task.Run(() => context.Schools.Update(school)));
    }

    public async Task DeleteSchoolAsync(School school)
    {
        await ModifySchoolAsync(async context => await Task.Run(() => context.Schools.Remove(school)));
    }

    private async Task ModifySchoolAsync(Func<LisaDbContext, Task> action)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        await action(context);
        await context.SaveChangesAsync();
        await _uiEventService.PublishAsync(UiEvents.SchoolsUpdated, _selectedSchool);
    }
}
