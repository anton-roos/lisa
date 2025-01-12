using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;
    private readonly IUiEventService _uiEventService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<User> _userManager;
    private bool _isInitialized = false;
    private School? _selectedSchool;

    public SchoolService(IDbContextFactory<LisaDbContext> dbContextFactory, IUiEventService uiEventService, AuthenticationStateProvider authenticationStateProvider, UserManager<User> userManager)
    {
        _dbContextFactory = dbContextFactory;
        _uiEventService = uiEventService;
        _authenticationStateProvider = authenticationStateProvider;
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        _userManager = userManager;
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        if (_isInitialized) return;

        var user = (await task).User;

        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            await InitializeSelectedSchoolAsync();
            _isInitialized = true;
        }
    }

    public async Task<School> SetCurrentSchool(Guid schoolId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _selectedSchool = await _context.Schools.SingleAsync(s => s.Id == schoolId);
        await _context.DisposeAsync();
        await _uiEventService.PublishAsync(UiEvents.SchoolSelected, _selectedSchool);
        return _selectedSchool;
    }

    public School GetSelectedSchool()
    {
        return _selectedSchool ?? new School { ShortName = "No Schools in DB", LongName = "No Schools in DB" };
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
                _selectedSchool = await _context.Schools.OrderBy(s => s.Id).FirstOrDefaultAsync()
                                  ?? new School { ShortName = "No Schools in DB", LongName = "No Schools in DB" };
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