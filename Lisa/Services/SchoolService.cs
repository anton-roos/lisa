using Lisa.Data;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService
{
    private School _currentSchool;
    private readonly LisaDbContext _context;
    public event Action? SchoolsUpdated;
    public event Action? SchoolSelected;

    public SchoolService(LisaDbContext context)
    {
        _context = context;
        _currentSchool = _context.Schools.FirstOrDefault() ?? new School { ShortName = "No School" };
    }

    public School GetCurrentSchool()
    {
        return _currentSchool;
    }

    public void SetCurrentSchool(Guid schoolId)
    {
        _currentSchool = _context.Schools.Single(s => s.Id == schoolId);
        SchoolSelected?.Invoke();
    }

    public async Task<School?> GetSchoolAsync(Guid id) => await _context.Schools.FindAsync(id);

    public async Task<List<School>> GetSchoolsAsync() => await _context.Schools.ToListAsync();

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
}