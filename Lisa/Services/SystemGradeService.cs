using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;
public class SystemGradeService(
    IDbContextFactory<LisaDbContext> dbContextFactory
)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<List<SystemGrade>> GetAllAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.SystemGrades
            .AsNoTracking()
            .OrderBy(s => s.SequenceNumber)
            .ToListAsync();
    }
}