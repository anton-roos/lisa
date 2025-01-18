using System.Security.Claims;
using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Lisa.Services;

public class SchoolService
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;
    private readonly IUiEventService _uiEventService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IJSRuntime _jsRuntime;

    private School? _selectedSchool;
    private static int _instanceCounter;
    private readonly int _instanceId;

    public SchoolService(
        IDbContextFactory<LisaDbContext> dbContextFactory,
        IUiEventService uiEventService,
        AuthenticationStateProvider authenticationStateProvider,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        ProtectedSessionStorage sessionStorage,
        IJSRuntime jsRuntime)
    {
        _dbContextFactory = dbContextFactory;
        _uiEventService = uiEventService;
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _sessionStorage = sessionStorage;
        _jsRuntime = jsRuntime;

        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        _instanceId = Interlocked.Increment(ref _instanceCounter);
        Console.WriteLine($"SchoolService created. ID: {_instanceId}");
    }

    public async Task InitializeAsync()
    {
        if (_selectedSchool != null) return;

        var result = await _sessionStorage.GetAsync<School>("selectedSchool");
        _selectedSchool = result.Success ? result.Value : await LoadDefaultSchoolAsync();
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var user = (await task).User;
        if (user.Identity?.IsAuthenticated == true)
        {
            await InitializeAsync();
        }
    }

    public async Task<School> SetCurrentSchoolAsync(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        _selectedSchool = await context.Schools.FindAsync(schoolId);
        await _sessionStorage.SetAsync("selectedSchool", _selectedSchool);
        await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
        return _selectedSchool!;
    }

    public async Task<IdentityUser<Guid>?> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId != null ? await _userManager.FindByIdAsync(userId) : null;
    }

    public School? GetSelectedSchool() => _selectedSchool;

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
        .Where(x=> x.Id == id)
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

    private async Task<School> LoadDefaultSchoolAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Schools.OrderBy(s => s.Id).FirstOrDefaultAsync()
               ?? new School { ShortName = "No School Assigned", LongName = "No School Assigned" };
    }
}
