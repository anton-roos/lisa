using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AssessmentTypeService
(
    IDbContextFactory<LisaDbContext> dbContextFactory
)   
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<List<AssessmentType>> GetAssessmentTypesAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.AssessmentTypes.ToListAsync();
    }

    public async Task<AssessmentType?> GetAssessmentTypeByIdAsync(int id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.AssessmentTypes.FindAsync(id);
    }

    public async Task<AssessmentType> CreateAssessmentTypeAsync(AssessmentType assessmentType)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.AssessmentTypes.Add(assessmentType);
        await context.SaveChangesAsync();
        return assessmentType;
    }

    public async Task<AssessmentType> UpdateAssessmentTypeAsync(AssessmentType assessmentType)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.AssessmentTypes.Update(assessmentType);
        await context.SaveChangesAsync();
        return assessmentType;
    }

    public async Task DeleteAssessmentTypeAsync(int id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var assessmentType = await context.AssessmentTypes.FindAsync(id);
        if (assessmentType != null)
        {
            context.AssessmentTypes.Remove(assessmentType);
            await context.SaveChangesAsync();
        }
    }
}