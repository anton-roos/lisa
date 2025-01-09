using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;
    private School? _selectedSchool;
    private readonly IUiEventService _uiEventService;

    public SchoolService(IDbContextFactory<LisaDbContext> dbContextFactory, IUiEventService uiEventService)
    {
        _dbContextFactory = dbContextFactory;
        InitializeSelectedSchool();
        _uiEventService = uiEventService;
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
            // Log the exception
            Console.WriteLine($"Failed to publish event: {ex.Message}");
        }
    }

    private void InitializeSelectedSchool()
    {
        var _context = _dbContextFactory.CreateDbContext();
        _selectedSchool = _context.Schools.FirstOrDefault() ?? new School { ShortName = "No Schools in DB", LongName = "No Schools in DB" };
        _context.Dispose();
    }
}