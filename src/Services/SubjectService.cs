using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SubjectService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<SubjectService> logger
)
{
    public async Task<List<Subject>> GetAllAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Subjects
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all subjects.");
            return [];
        }
    }

    public async Task<List<Subject>> GetAllCombinationSubjectsAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Subjects
                .Where(s => s.SubjectType == SubjectType.Combination)
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all subjects.");
            return [];
        }
    }

    public async Task<Subject?> GetByCodeAsync(string code)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Code == code);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subject by code: {SubjectCode}", code);
            return null;
        }
    }

    public async Task<ICollection<Subject>> GetSubjectsForGradeAsync(Guid gradeId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var grade = await context.SchoolGrades.FindAsync(gradeId);

            return await context.Subjects
                .Where(s => grade != null && s.GradesApplicable != null &&
                            s.GradesApplicable.Any(g => g == grade.SystemGrade.SequenceNumber))
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subjects for grade: {GradeId}", gradeId);
            return [];
        }
    }

    public async Task<bool> CreateAsync(Subject subject)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            await context.Subjects.AddAsync(subject);
            await context.SaveChangesAsync();
            logger.LogInformation("Created new subject: {SubjectId}", subject.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating subject.");
            return false;
        }
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subject with ID: {SubjectId}", id);
            return null;
        }
    }

    public async Task<bool> UpdateAsync(Subject subject)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.Subjects.FindAsync(subject.Id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent subject. SubjectId: {SubjectId}", subject.Id);
                return false;
            }

            existing.Name = subject.Name;
            existing.Code = subject.Code;
            existing.Description = subject.Description;

            context.Entry(existing).State = EntityState.Modified;
            await context.SaveChangesAsync();
            logger.LogInformation("Updated subject: {SubjectId}", subject.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating subject with ID: {SubjectId}", subject.Id);
            return false;
        }
    }

    public async Task<List<Subject>> GetMathSubjectsAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Subjects
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .Where(s => s.SubjectType == SubjectType.MathCombination)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching math subjects.");
            return [];
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.Subjects.FindAsync(id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to delete non-existent subject. SubjectId: {SubjectId}", id);
                return false;
            }

            context.Subjects.Remove(existing);
            await context.SaveChangesAsync();
            logger.LogInformation("Deleted subject: {SubjectId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting subject with ID: {SubjectId}", id);
            return false;
        }
    }

    public async Task UpdateOrderAsync(List<Subject> subjects)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        foreach (var subject in subjects)
        {
            context.Subjects.Update(subject);
        }

        await context.SaveChangesAsync();
    }
}