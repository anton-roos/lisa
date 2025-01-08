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

    public async Task<Combination> GetByIdAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Combinations
            .Include(c => c.Grade)
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Combination> UpdateAsync(Guid id, Combination combination)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existingCombination = await _context.Combinations
            .Include(c => c.Grade)
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCombination == null)
        {
            return null;
        }

        existingCombination.Name = combination.Name;
        existingCombination.GradeId = combination.GradeId;
        existingCombination.Grade = combination.Grade;
        existingCombination.Subjects = combination.Subjects;

        await _context.SaveChangesAsync();
        return existingCombination;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existingCombination = await _context.Combinations
            .Include(c => c.Grade)
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCombination == null)
        {
            return false;
        }

        _context.Combinations.Remove(existingCombination);
        await _context.SaveChangesAsync();
        return true;
    }
}
