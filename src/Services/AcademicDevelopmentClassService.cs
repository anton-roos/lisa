using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AcademicDevelopmentClassService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<AcademicDevelopmentClassService> logger
)
{
    public async Task<List<AcademicDevelopmentClass>> GetAllAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.AcademicDevelopmentClasses
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .AsNoTracking()
                .OrderBy(adc => adc.DateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all academic development classes.");
            return [];
        }
    }

    public async Task<List<AcademicDevelopmentClass>> GetBySchoolIdAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.AcademicDevelopmentClasses
                .Where(adc => adc.SchoolId == schoolId)
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .AsNoTracking()
                .OrderBy(adc => adc.DateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching academic development classes for school {SchoolId}.", schoolId);
            return [];
        }
    }

    public async Task<AcademicDevelopmentClass?> GetByIdAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.AcademicDevelopmentClasses
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .AsNoTracking()
                .FirstOrDefaultAsync(adc => adc.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching academic development class with ID: {Id}", id);
            return null;
        }
    }

    public async Task<AcademicDevelopmentClass> CreateAsync(AcademicDevelopmentClass academicDevelopmentClass)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            academicDevelopmentClass.Id = Guid.NewGuid();
            
            // Ensure DateTime is in UTC
            if (academicDevelopmentClass.DateTime.Kind == DateTimeKind.Unspecified)
            {
                academicDevelopmentClass.DateTime = DateTime.SpecifyKind(academicDevelopmentClass.DateTime, DateTimeKind.Utc);
            }
            else if (academicDevelopmentClass.DateTime.Kind == DateTimeKind.Local)
            {
                academicDevelopmentClass.DateTime = academicDevelopmentClass.DateTime.ToUniversalTime();
            }

            academicDevelopmentClass.CreatedAt = DateTime.UtcNow;
            academicDevelopmentClass.UpdatedAt = DateTime.UtcNow;

            await context.AcademicDevelopmentClasses.AddAsync(academicDevelopmentClass);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Created new academic development class: {Id}", academicDevelopmentClass.Id);
            return academicDevelopmentClass;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating academic development class.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(AcademicDevelopmentClass academicDevelopmentClass)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.AcademicDevelopmentClasses.FindAsync(academicDevelopmentClass.Id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent academic development class {Id}.", academicDevelopmentClass.Id);
                return false;
            }

            existing.DateTime = academicDevelopmentClass.DateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(academicDevelopmentClass.DateTime, DateTimeKind.Utc)
                : academicDevelopmentClass.DateTime.Kind == DateTimeKind.Local 
                    ? academicDevelopmentClass.DateTime.ToUniversalTime()
                    : academicDevelopmentClass.DateTime;
            existing.SchoolGradeId = academicDevelopmentClass.SchoolGradeId;
            existing.SubjectId = academicDevelopmentClass.SubjectId;
            existing.TeacherId = academicDevelopmentClass.TeacherId;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = academicDevelopmentClass.UpdatedBy;

            context.Entry(existing).State = EntityState.Modified;
            await context.SaveChangesAsync();
            
            logger.LogInformation("Updated academic development class: {Id}", academicDevelopmentClass.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating academic development class: {Id}", academicDevelopmentClass.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.AcademicDevelopmentClasses.FindAsync(id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to delete non-existent academic development class {Id}.", id);
                return false;
            }

            context.AcademicDevelopmentClasses.Remove(existing);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Deleted academic development class: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting academic development class: {Id}", id);
            return false;
        }
    }
}
