using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CareGroupService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<CareGroup> CreateAsync(CareGroup careGroup)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.CareGroups.Add(careGroup);
        await _context.SaveChangesAsync();
        return careGroup;
    }

    public async Task<IEnumerable<CareGroup>> GetAllAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.CareGroups
            .Include(c => c.CareGroupMembers)
            .ToListAsync();
    }

    public async Task<CareGroup> GetByIdAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        
        return (await _context.CareGroups
            .Include(c => c.CareGroupMembers)
            .FirstOrDefaultAsync(c => c.Id == id))!;
    }

    public async Task<CareGroup> UpdateAsync(CareGroup careGroup)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.CareGroups.Update(careGroup);
        await _context.SaveChangesAsync();
        return careGroup;
    }

    public async Task DeleteAsync(CareGroup careGroup)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.CareGroups.Remove(careGroup);
        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
    }
}
