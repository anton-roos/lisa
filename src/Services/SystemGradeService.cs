using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;
public class SystemGradeService(
    IDbContextFactory<LisaDbContext> dbContextFactory
)
{
    public async Task<List<SystemGrade>> GetAllAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.SystemGrades
            .AsNoTracking()
            .OrderBy(s => s.SequenceNumber)
            .ToListAsync();
    }
}