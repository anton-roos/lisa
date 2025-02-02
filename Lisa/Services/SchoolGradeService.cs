using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class GradeService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<GradeService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<GradeService> _logger = logger;

    /// <summary>
    /// Creates a new grade.
    /// </summary>
    public async Task<Grade> CreateGradeAsync(Grade grade)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.Grades.AddAsync(grade);
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
    public async Task<Grade?> GetByIdAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Grades
            .AsNoTracking()
            .FirstOrDefaultAsync(grade => grade.Id == id);
    }

    /// <summary>
    /// Retrieves a grade with its register classes and learners.
    /// </summary>
    public async Task<Grade?> GetGradeWithRegisterClassesAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Grades
            .AsNoTracking()
            .Include(g => g.RegisterClasses!)
                .ThenInclude(rc => rc.Learners)
            .Include(g => g.Combinations)
            .FirstOrDefaultAsync(grade => grade.Id == id);
    }

    /// <summary>
    /// Retrieves all grades.
    /// </summary>
    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Grades
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing grade.
    /// </summary>
    public async Task<bool> UpdateGradeAsync(Grade grade)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingGrade = await context.Grades.FindAsync(grade.Id);

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
            existingGrade.Name = grade.Name;
            existingGrade.RegisterClasses = grade.RegisterClasses;
            existingGrade.SequenceNumber = grade.SequenceNumber;

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
    public async Task<bool> DeleteGradeAsync(Guid id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var grade = await context.Grades.FindAsync(id);
            if (grade == null)
            {
                _logger.LogWarning("Attempted to delete grade {GradeId}, but it does not exist.", id);
                return false;
            }

            context.Grades.Remove(grade);
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
    public async Task<List<Grade>> GetGradesForSchool(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Grades
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }
}
