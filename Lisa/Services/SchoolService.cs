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
    private School? _selectedSchool;
    private static int _instanceCounter = 0;
    private int _instanceId;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IJSRuntime _jsRuntime;

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
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _instanceId = Interlocked.Increment(ref _instanceCounter);
        Console.WriteLine($"SchoolService created. ID: {_instanceId}");
        _sessionStorage = sessionStorage;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        if (_selectedSchool != null) return;

        
        var result = await _sessionStorage.GetAsync<School>("selectedSchool");

        if (result.Success)
        {
            _selectedSchool = result.Value;
        }
        else
        {
            await InitializeSelectedSchoolAsync();
        }

        var user = await GetCurrentUserAsync();

        if (user != null)
        {
            if (user is User loggedInUser && loggedInUser.School != null)
            {
                _selectedSchool = loggedInUser.School;
            }
            else if (user is User appUser && await _userManager.IsInRoleAsync(appUser, Roles.SystemAdministrator))
            {
                var context = _dbContextFactory.CreateDbContext();
                _selectedSchool = await context.Schools.OrderBy(s => s.Id).FirstOrDefaultAsync();
                context.Dispose();
            }
            else
            {
                _selectedSchool = new School { ShortName = "No School Assigned", LongName = "No School Assigned" };
            }
        }
        else
        {
            var context = _dbContextFactory.CreateDbContext();
            _selectedSchool = await context.Schools.OrderBy(s => s.Id).FirstOrDefaultAsync();
            context.Dispose();
        }
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var user = (await task).User;

        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            await InitializeSelectedSchoolAsync();
        }
    }

    public async Task<School> SetCurrentSchool(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        _selectedSchool = await context.Schools.SingleAsync(s => s.Id == schoolId);
        await _sessionStorage.SetAsync("selectedSchool", _selectedSchool);
        await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
        return _selectedSchool;
    }

    public async Task<IdentityUser<Guid>?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _userManager.FindByIdAsync(userId!);
    }

    public async Task<School>? GetSelectedSchool()
    {
        if (_jsRuntime is IJSInProcessRuntime && _selectedSchool is null)
        {
            var result = await _sessionStorage.GetAsync<School>("selectedSchool");
            _selectedSchool = result.Success ? result.Value : new School { ShortName = "Default" };
            return _selectedSchool;
        }
        else
        {
            return _selectedSchool;
        }
    }

    public async Task<School?> GetSchoolAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Schools.FindAsync(id);
    }

    public async Task<List<School>> GetAllAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        var schools = await _context.Schools.ToListAsync();
        _context.Dispose();
        return schools;
    }

    public async Task<List<SchoolType>> GetSchoolTypesAsync()
    {

        var _context = _dbContextFactory.CreateDbContext();
        return await _context.SchoolTypes.ToListAsync();
    }

    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.SchoolCurriculums.ToListAsync();
    }

    public async Task DeleteAsync(School school)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Schools.Remove(school);
        await _context.SaveChangesAsync();
        _context.Dispose();
        await _uiEventService.PublishAsync(UiEvents.SchoolsUpdated, school);
    }

    public async Task AddAsync(School school)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        _context.Dispose();
        await _uiEventService.PublishAsync(UiEvents.SchoolsUpdated, school);
    }

    public async Task UpdateAsync(School school)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
        try
        {
            await _uiEventService.PublishAsync(UiEvents.SchoolsUpdated, school);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish event: {ex.Message}");
        }
    }

    private async Task InitializeSelectedSchoolAsync()
    {
        using var _context = _dbContextFactory.CreateDbContext();

        var user = await _userManager.GetUserAsync(_authenticationStateProvider.GetAuthenticationStateAsync().Result.User);

        if (user != null)
        {
            if (await _userManager.IsInRoleAsync(user, Roles.SystemAdministrator))
            {
                _selectedSchool = await _context.Schools.OrderBy(s => s.Id).FirstOrDefaultAsync();
                await _sessionStorage.SetAsync("selectedSchool", _selectedSchool);

            }
            else
            {
                var loggedInUser = await _context.Users
                    .Include(t => t.School)
                    .FirstOrDefaultAsync(t => t.Id == user.Id);

                _selectedSchool = loggedInUser?.School
                    ?? new School { ShortName = "No School Assigned", LongName = "No School Assigned" };
            }
        }
        else
        {
            _selectedSchool = new School { ShortName = "Not Logged In", LongName = "User Not Logged In" };
        }
    }
}