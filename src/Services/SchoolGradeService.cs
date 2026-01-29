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
    public async Task<SchoolGrade> CreateAsync(SchoolGrade grade)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            await context.SchoolGrades.AddAsync(grade);
            await context.SaveChangesAsync();
            logger.LogInformation("Created a new grade: {GradeId}", grade.Id);
            return grade;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating grade.");
            throw;
        }
    }

    public async Task<SchoolGrade?> UpdateAsync(SchoolGrade grade)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existingGrade = await context.SchoolGrades.FindAsync(grade.Id);
            if (existingGrade == null)
            {
                logger.LogWarning("Attempted to update grade {GradeId}, but it does not exist.", grade.Id);
                return null;
            }

            // Validate times
            if (grade.StartTime.HasValue && grade.EndTime.HasValue && grade.StartTime >= grade.EndTime)
            {
                logger.LogWarning("Invalid times for grade {GradeId}: Start time must be before end time", grade.Id);
                throw new InvalidOperationException("Start time must be before end time");
            }

            existingGrade.StartTime = grade.StartTime;
            existingGrade.EndTime = grade.EndTime;

            await context.SaveChangesAsync();
            logger.LogInformation("Updated grade {GradeId}: Start={StartTime}, End={EndTime}", 
                grade.Id, grade.StartTime?.ToString("HH:mm"), grade.EndTime?.ToString("HH:mm"));
            return existingGrade;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating grade: {GradeId}", grade.Id);
            throw new InvalidOperationException("The grade was modified by another user. Please refresh and try again.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating grade: {GradeId}", grade.Id);
            throw;
        }
    }

    public async Task<SchoolGrade?> GetByIdAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
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
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var grade = await context.SchoolGrades.FindAsync(id);
            if (grade == null)
            {
                logger.LogWarning("Attempted to delete grade {GradeId}, but it does not exist.", id);
                return false;
            }

            context.SchoolGrades.Remove(grade);
            await context.SaveChangesAsync();
            logger.LogInformation("Deleted grade: {GradeId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting grade: {GradeId}", id);
            return false;
        }
    }

    public async Task<List<SchoolGrade>> GetGradesForSchool(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SchoolId == schoolId)
            .OrderBy(s => s.SystemGrade.SequenceNumber)
            .ToListAsync();
    }

    public async Task<List<SchoolGrade>> GetCombinationGradesForSchool(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SchoolId == schoolId && s.SystemGrade.CombinationGrade)
            .OrderBy(s => s.SystemGrade.SequenceNumber)
            .ToListAsync();
    }

    public async Task<SchoolGrade?> GetBySystemGradeAndSchoolAsync(int systemGradeId, Guid selectedSchoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SchoolGrades
            .AsNoTracking()
            .Include(g => g.SystemGrade)
            .Where(s => s.SystemGradeId == systemGradeId && s.SchoolId == selectedSchoolId)
            .FirstOrDefaultAsync();
    }
}