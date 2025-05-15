using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolGradeService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<SchoolGradeService> logger
)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<SchoolGradeService> _logger = logger;

    public async Task<SchoolGrade> CreateAsync(SchoolGrade grade)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.SchoolGrades.AddAsync(grade);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created a new grade: {GradeId}", grade.Id);
            return grade;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating grade.");
            throw;
        }
    }

    public async Task<SchoolGrade?> GetByIdAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Include(g => g.RegisterClasses)
            .Include(g => g.Combinations)
            .FirstOrDefaultAsync(grade => grade.Id == id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var grade = await context.SchoolGrades.FindAsync(id);
            if (grade == null)
            {
                _logger.LogWarning("Attempted to delete grade {GradeId}, but it does not exist.", id);
                return false;
            }

            context.SchoolGrades.Remove(grade);
            await context.SaveChangesAsync();
            _logger.LogInformation("Deleted grade: {GradeId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting grade: {GradeId}", id);
            return false;
        }
    }

    public async Task<List<SchoolGrade>> GetGradesForSchool(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SchoolId == schoolId)
            .OrderBy(s => s.SystemGrade.SequenceNumber)
            .ToListAsync();
    }

    public async Task<List<SchoolGrade>> GetCombinationGradesForSchool(Guid schoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SchoolId == schoolId && s.SystemGrade.CombinationGrade)
            .OrderBy(s => s.SystemGrade.SequenceNumber)
            .ToListAsync();
    }

    public async Task<SchoolGrade?> GetBySystemGradeAndSchoolAsync(int systemGradeId, Guid selectedSchoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SystemGradeId == systemGradeId && s.SchoolId == selectedSchoolId)
            .FirstOrDefaultAsync();
    }
}