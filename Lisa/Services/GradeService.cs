using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class GradeService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<Grade> CreateGradeAsync(Grade grade)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task<Grade?> GetByIdAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Grades
            .FirstOrDefaultAsync(grade => grade.Id == id);
    }

    public async Task<Grade?> GetGradeWithRegisterClassesAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Grades
            .Include(g => g.RegisterClasses!)
            .ThenInclude(rc => rc.Learners)
            .Include(g => g.Combinations)
            .FirstOrDefaultAsync(grade => grade.Id == id);
    }

    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Grades
            .ToListAsync();
    }

    public async Task<Grade> UpdateGradeAsync(Grade grade)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Grades.Update(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task DeleteGradeAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var grade = await GetByIdAsync(id);
        if (grade is null)
        {
            return;
        }

        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Grade>> GetGradesForSchool(Guid schoolId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var grades = await _context.Grades
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
        return grades;
    }
}