using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CombinationService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<Combination> CreateAsync(Combination subjectCombination)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Combinations.Add(subjectCombination);
        await _context.SaveChangesAsync();
        return subjectCombination;
    }

    public async Task<Combination> GetByIdAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var combination = await _context.Combinations
            .Include(sc => sc.Subjects)
            .Include(sc => sc.Grade)
                .ThenInclude(g => g!.School)
            .FirstOrDefaultAsync(sc => sc.Id == id);

        if (combination == null)
        {
            throw new KeyNotFoundException($"Combination with id {id} not found.");
        }

        return combination;
    }

    public async Task<IEnumerable<Combination>> GetAllAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Combinations
            .Include(sc => sc.Subjects)
            .ToListAsync();
    }

    public async Task<IEnumerable<Combination>> GetSubjectCombinationsForSchool(School school)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Combinations
            .Include(sc => sc.Subjects)
            .Include(sc => sc.Grade)
                .ThenInclude(g => g!.School)
            .Where(sc => sc.Grade!.SchoolId == school.Id)
            .ToListAsync();
    }

    public async Task<Combination> UpdateAsync(Combination subjectCombination)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Combinations.Update(subjectCombination);
        await _context.SaveChangesAsync();
        return subjectCombination;
    }

    public async Task DeleteAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var subjectCombination = await _context.Combinations.FindAsync(id);
        if (subjectCombination != null)
        {
            _context.Combinations.Remove(subjectCombination);
        }
        else
        {
            throw new KeyNotFoundException($"Combination with id {id} not found.");
        }
        await _context.SaveChangesAsync();
    }
}