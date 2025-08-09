using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CareGroupService
(
    IDbContextFactory<LisaDbContext> dbContextFactory
)
{
    public async Task AddUserToCareGroupAsync(Guid careGroupId, Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

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
        await using var context = await dbContextFactory.CreateDbContextAsync();

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

    public async Task CreateAsync(CareGroup careGroup, List<Guid> userIds)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.CareGroups.AddAsync(careGroup);
        await context.SaveChangesAsync();

        var users = await context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        careGroup.Users = users;

        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CareGroup careGroup, List<Guid> userIds)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var existingCareGroup = await context.CareGroups
            .Include(cg => cg.Users)
            .FirstOrDefaultAsync(cg => cg.Id == careGroup.Id)
            ?? throw new ArgumentException("Care group not found.");

        existingCareGroup.Name = careGroup.Name;

        var users = await context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        existingCareGroup.Users = users;

        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CareGroup>> GetAllAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .AsNoTracking()
            .Include(c => c.CareGroupMembers)
            .Include(c => c.Users)
            .Include(c => c.Users)
            .ToListAsync();
    }

    public async Task<List<CareGroup>> GetBySchoolAsync(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .AsNoTracking()
            .Include(c => c.CareGroupMembers)
            .Where(c => c.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<CareGroup?> GetByIdAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .Include(c => c.CareGroupMembers)
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task DeleteAsync(CareGroup careGroup)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.CareGroups.Remove(careGroup);
        await context.SaveChangesAsync();
    }
}
