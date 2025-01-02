using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService(LisaDbContext context, IServiceProvider serviceProvider)
{
    private School? _selectedSchool;
    private List<School>? _schools;
    private readonly LisaDbContext _context = context;
    public event Action? SchoolsUpdated;
    public event Action? SchoolSelected;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public School? SelectedSchool
    {
        get
        {
            if (_selectedSchool == null)
            {
                InitializeSelectedSchool();
            }
            return _selectedSchool;
        }
        set
        {
            _selectedSchool = value;
            SchoolSelected?.Invoke();
        }
    }

    public List<School>? Schools
    {
        get
        {
            if (_schools == null)
            {
                IntiliazeSchools();
            }
            return _schools;
        }
    }

    public void SetCurrentSchool(Guid schoolId)
    {
        _selectedSchool = _context.Schools.Single(s => s.Id == schoolId);
        SchoolSelected?.Invoke();
    }

    public async Task<School?> GetSchoolAsync(Guid id) => await _context.Schools.FindAsync(id);

    public async Task<List<SchoolType>> GetSchoolTypesAsync() => await _context.SchoolTypes.ToListAsync();

    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync() => await _context.SchoolCurriculums.ToListAsync();

    public async Task DeleteSchoolAsync(School school)
    {
        _context.Schools.Remove(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    public async Task AddSchoolAsync(School school)
    {
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    public async Task UpdateSchoolAsync(School school)
    {
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    private void InitializeSelectedSchool()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();

        _selectedSchool = dbContext.Schools.FirstOrDefault();
        SchoolSelected?.Invoke();
        scope.Dispose();
    }

    public void IntiliazeSchools()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        _schools = [.. dbContext.Schools];
        if (_schools != null)
        {
            SchoolsUpdated?.Invoke();
        }
        else
        {
            _schools = [new School { ShortName = "No Schools in DB" }];
        }
        scope.Dispose();
    }
}