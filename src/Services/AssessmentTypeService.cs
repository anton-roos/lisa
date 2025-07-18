using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AssessmentTypeService
(
    IDbContextFactory<LisaDbContext> dbContextFactory
)
{
    public async Task<List<AssessmentType>> GetAssessmentTypesAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AssessmentTypes.ToListAsync();
    }

    public async Task<AssessmentType?> GetAssessmentTypeByIdAsync(int id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AssessmentTypes.FindAsync(id);
    }

    public async Task<AssessmentType> CreateAssessmentTypeAsync(AssessmentType assessmentType)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.AssessmentTypes.Add(assessmentType);
        await context.SaveChangesAsync();
        return assessmentType;
    }

    public async Task<AssessmentType> UpdateAssessmentTypeAsync(AssessmentType assessmentType)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.AssessmentTypes.Update(assessmentType);
        await context.SaveChangesAsync();
        return assessmentType;
    }

    public async Task DeleteAssessmentTypeAsync(int id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var assessmentType = await context.AssessmentTypes.FindAsync(id);
        if (assessmentType != null)
        {
            context.AssessmentTypes.Remove(assessmentType);
            await context.SaveChangesAsync();
        }
    }
}