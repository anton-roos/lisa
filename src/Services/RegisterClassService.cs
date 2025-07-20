using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class RegisterClassService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<RegisterClassService> logger
)
{
    public async Task<RegisterClass?> GetByIdAsync(Guid registerClassId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.SchoolGrade!)
                .Include(rc => rc.User!)
                .Include(rc => rc.CompulsorySubjects!)
                .Include(rc => rc.Learners!)
                .FirstOrDefaultAsync(rc => rc.Id == registerClassId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching RegisterClass with ID: {RegisterClassId}", registerClassId);
            return null;
        }
    }
    public async Task<List<RegisterClass>> GetByGradeIdAsync(Guid gradeId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .Where(rc => rc.SchoolGradeId == gradeId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching RegisterClass with ID: {schoolId}", gradeId);
            return [];
        }
    }

    public async Task<List<RegisterClass>> GetBySchoolIdAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .Where(rc => rc.SchoolGrade != null && rc.SchoolGrade.SchoolId == schoolId)
                .Include(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching RegisterClass with ID: {schoolId}", schoolId);
            return [];
        }
    }

    public async Task<List<RegisterClass>> GetAllAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.SchoolGrade!)
                .ThenInclude(g => g.School!)
                .Include(rc => rc.SchoolGrade!)
                .ThenInclude(g => g.SystemGrade)
                .Include(rc => rc.User!)
                .Include(rc => rc.CompulsorySubjects!)
                .Include(rc => rc.Learners!)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all RegisterClasses.");
            return [];
        }
    }

    public async Task<bool> UpdateAsync(RegisterClass registerClass)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.RegisterClasses.FindAsync(registerClass.Id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent RegisterClass {RegisterClassId}.",
                    registerClass.Id);
                return false;
            }

            existing.Name = registerClass.Name;
            existing.UserId = registerClass.UserId;
            existing.SchoolGradeId = registerClass.SchoolGradeId;
            existing.CompulsorySubjects = registerClass.CompulsorySubjects;

            context.Entry(existing).State = EntityState.Modified;
            await context.SaveChangesAsync();
            logger.LogInformation("Updated RegisterClass: {RegisterClassId}", registerClass.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating RegisterClass: {RegisterClassId}", registerClass.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid registerClassId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var registerClass = await context.RegisterClasses.FindAsync(registerClassId);

            if (registerClass == null)
            {
                logger.LogWarning("Attempted to delete non-existent RegisterClass {RegisterClassId}.",
                    registerClassId);
                return false;
            }

            context.RegisterClasses.Remove(registerClass);
            await context.SaveChangesAsync();
            logger.LogInformation("Deleted RegisterClass: {RegisterClassId}", registerClassId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting RegisterClass: {RegisterClassId}", registerClassId);
            return false;
        }
    }

    public async Task<bool> CreateAsync(RegisterClassViewModel model, Guid? registerClassId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var selectedSubjects = await context.Subjects
                .Where(s => model.SubjectIds.Contains(s.Id))
                .ToListAsync();

            RegisterClass? registerClass;

            if (registerClassId.HasValue)
            {
                registerClass = await context.RegisterClasses
                    .Include(rc => rc.CompulsorySubjects)
                    .FirstOrDefaultAsync(rc => rc.Id == registerClassId.Value);

                if (registerClass == null)
                {
                    logger.LogWarning("Attempted to update non-existent RegisterClass {RegisterClassId}.", registerClassId.Value);
                    return false;
                }

                registerClass.Name = model.Name;
                registerClass.SchoolGradeId = model.GradeId;
                registerClass.UserId = model.TeacherId;

                registerClass.CompulsorySubjects?.Clear();

                foreach (var subject in selectedSubjects)
                {
                    registerClass.CompulsorySubjects?.Add(subject);
                }
            }
            else
            {
                registerClass = new RegisterClass
                {
                    Name = model.Name,
                    SchoolGradeId = model.GradeId,
                    CompulsorySubjects = selectedSubjects,
                    UserId = model.TeacherId,
                };

                await context.RegisterClasses.AddAsync(registerClass);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Register Class {Action} successfully: {RegisterClassId}", registerClassId.HasValue ? "updated" : "created", registerClass.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving RegisterClass.");
            return false;
        }
    }
}
