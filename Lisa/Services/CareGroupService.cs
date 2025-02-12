using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CareGroupService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task AddUserToCareGroupAsync(Guid careGroupId, Guid userId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var careGroup = await context.CareGroups
            .Include(cg => cg.Users)
            .FirstOrDefaultAsync(cg => cg.Id == careGroupId) 
                ?? throw new ArgumentException("Care group not found.");
        
        var user = await context.Users.FindAsync(userId) 
            ?? throw new ArgumentException("User not found.");
        
        if (careGroup.Users != null && careGroup.Users.Any(u => u.Id == user.Id))
        {
            throw new InvalidOperationException("User is already in this care group.");
        }

        careGroup.Users ??= [];

        if (!careGroup.Users.Any(u => u.Id == user.Id))
        {
            careGroup.Users.Add(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task RemoveUserFromCareGroupAsync(Guid careGroupId, Guid userId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var careGroup = await context.CareGroups
            .Include(cg => cg.Users)
            .FirstOrDefaultAsync(cg => cg.Id == careGroupId);
        if (careGroup != null)
        {
            var user = careGroup.Users?.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                careGroup.Users?.Remove(user);
                await context.SaveChangesAsync();
            }
        }
    }

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

        var trackedCareGroup = await context.CareGroups
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == careGroup.Id)
        ?? throw new ArgumentException("Care group not found.");

        trackedCareGroup.Name = careGroup.Name;

        await context.SaveChangesAsync();
        return trackedCareGroup;
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
