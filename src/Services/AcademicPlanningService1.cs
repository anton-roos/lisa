using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AcademicPlanningService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<AcademicPlanningService> logger
)
{
    public async Task<AcademicPlan> CreateAsync(AcademicPlan academicPlan)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            await context.AcademicPlans.AddAsync(academicPlan);
            await context.SaveChangesAsync();
            logger.LogInformation("Created a new academic plan: {AcademicPlanId}", academicPlan.Id);
            return academicPlan;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating academic plan.");
            throw;
        }
    }

    public async Task<List<AcademicPlan>> GetAllAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AcademicPlans
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AcademicPlan?> GetByIdAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AcademicPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(plan => plan.Id == id);
    }


    public async Task<AcademicPlan?> UpdateAsync(AcademicPlan academicPlan)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existingPlan = await context.AcademicPlans.FindAsync(academicPlan.Id);
            if (existingPlan == null)
            {
                logger.LogWarning("Attempted to update academic plan {AcademicPlanId}, but it does not exist.", academicPlan.Id);
                return null;
            }

            existingPlan.Name = academicPlan.Name;

            await context.SaveChangesAsync();
            logger.LogInformation("Updated academic plan: {AcademicPlanId}", academicPlan.Id);
            return existingPlan;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating academic plan: {AcademicPlanId}", academicPlan.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var academicPlan = await context.AcademicPlans.FindAsync(id);
            if (academicPlan == null)
            {
                logger.LogWarning("Attempted to delete academic plan {AcademicPlanId}, but it does not exist.", id);
                return false;
            }

            context.AcademicPlans.Remove(academicPlan);
            await context.SaveChangesAsync();
            logger.LogInformation("Deleted academic plan: {AcademicPlanId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting academic plan: {AcademicPlanId}", id);
            return false;
        }
    }
}