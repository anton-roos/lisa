using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolGradeService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<SchoolGradeService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<SchoolGradeService> _logger = logger;

    /// <summary>
    /// Creates a new grade.
    /// </summary>
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

    /// <summary>
    /// Retrieves a grade by ID.
    /// </summary>
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

    /// <summary>
    /// Retrieves a grade with its register classes and learners.
    /// </summary>
    public async Task<SchoolGrade?> GetGradeWithRegisterClassesAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Include(g => g.RegisterClasses!)
                .ThenInclude(rc => rc.Learners)
            .Include(g => g.Combinations)
            .FirstOrDefaultAsync(grade => grade.Id == id);
    }

    /// <summary>
    /// Retrieves all grades.
    /// </summary>
    public async Task<IEnumerable<SchoolGrade>> GetAllAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing grade.
    /// </summary>
    public async Task<bool> UpdateGradeAsync(SchoolGrade grade)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingGrade = await context.SchoolGrades.FindAsync(grade.Id);

            if (existingGrade == null)
            {
                _logger.LogWarning("Attempted to update grade {GradeId}, but it does not exist.", grade.Id);
                return false;
            }

            // TODO: Come and take a look at all of these properties
            // and if they are being upated correctly.
            // do we need to update the register classes?
            // do we need to update the sequence number?
            // check the UI and see what makes sense.
            existingGrade.SystemGradeId = grade.SystemGradeId;
            existingGrade.RegisterClasses = grade.RegisterClasses;

            context.Entry(existingGrade).State = EntityState.Modified;
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated grade: {GradeId}", grade.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating grade: {GradeId}", grade.Id);
            return false;
        }
    }

    /// <summary>
    /// Deletes a grade by ID.
    /// </summary>
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

    /// <summary>
    /// Retrieves all grades for a given school.
    /// </summary>
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

    /// <summary>
    /// Retrieves all combination grades for a given school.
    /// </summary>
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

    public async Task<SchoolGrade?> GetBySystemGradeAndSchoolAsync(int systemGradeId, Guid SelectedSchoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SystemGradeId == systemGradeId && s.SchoolId == SelectedSchoolId)
            .FirstOrDefaultAsync();
    }
}
