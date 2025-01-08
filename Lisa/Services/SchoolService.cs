using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;
    private School? _selectedSchool;
    public event Action? SchoolsUpdated;
    public event Action? SchoolSelected;
    public School SelectedSchool { get; set; }

    public SchoolService(IDbContextFactory<LisaDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        InitializeSelectedSchool();
        SelectedSchool = _selectedSchool ?? new School { ShortName = "No Schools in DB" };
    }

    public void SetCurrentSchool(Guid schoolId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _selectedSchool = _context.Schools.Single(s => s.Id == schoolId);
        SchoolSelected?.Invoke();
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
        SchoolsUpdated?.Invoke();
    }

    public async Task AddAsync(School school)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    public async Task UpdateAsync(School school)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    private void InitializeSelectedSchool()
    {
        var _context = _dbContextFactory.CreateDbContext();
        _selectedSchool = _context.Schools.FirstOrDefault() ?? new School { ShortName = "No Schools in DB", LongName = "No Schools in DB" };
        _context.Dispose();
        SchoolSelected?.Invoke();
    }
}