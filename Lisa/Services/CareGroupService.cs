using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CareGroupService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    /// <summary>
    /// Create a new CareGroup
    /// </summary>
    public async Task<CareGroup> CreateAsync(CareGroup careGroup)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.CareGroups.AddAsync(careGroup);
        await context.SaveChangesAsync();
        return careGroup;
    }

    /// <summary>
    /// Retrieve all CareGroups with members
    /// </summary>
    public async Task<IEnumerable<CareGroup>> GetAllAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .AsNoTracking()
            .Include(c => c.CareGroupMembers)
            .Include(c => c.Users)
            .ToListAsync();
    }

    public async Task<List<CareGroup>> GetBySchoolAsync(Guid schoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .AsNoTracking()
            .Include(c => c.CareGroupMembers)
            .Where(c => c.SchoolId == schoolId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieve a specific CareGroup by ID
    /// </summary>
    public async Task<CareGroup?> GetByIdAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .AsNoTracking()
            .Include(c => c.CareGroupMembers)
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Update an existing CareGroup
    /// </summary>
    public async Task<CareGroup> UpdateAsync(CareGroup careGroup)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.CareGroups.Update(careGroup);
        await context.SaveChangesAsync();
        return careGroup;
    }

    /// <summary>
    /// Delete a CareGroup
    /// </summary>
    public async Task DeleteAsync(CareGroup careGroup)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.CareGroups.Remove(careGroup);
        await context.SaveChangesAsync();
    }
}
