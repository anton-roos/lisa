using Lisa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Lisa.Domain.Entities;

namespace Lisa.Infrastructure.Services;
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
