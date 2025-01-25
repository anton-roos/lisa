using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
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
    public async Task<List<Combination>> GetCombinationsBySchoolId(Guid schoolId)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.Combinations
                            .Where(c => c.Grade!.SchoolId == schoolId)
                            .Include(c => c.Subjects)
                            .ToListAsync();
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

    public async Task AddCombinationAsync(CombinationViewModel model, IEnumerable<Subject> selectedSubjects)
    {
        using var _context = _dbContextFactory.CreateDbContext();

        foreach (var subject in selectedSubjects)
        {
            _context.Attach(subject);
        }

        var newCombination = new Combination
        {
            Name = model.Name,
            GradeId = model.GradeId,
            CombinationType = model.CombinationType,
            Subjects = selectedSubjects.ToList()
        };

        _context.Combinations.Add(newCombination);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCombinationAsync(CombinationViewModel model, IEnumerable<Subject> selectedSubjects)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existing = await _context.Combinations
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == model.Id);

        if (existing == null)
            throw new InvalidOperationException("Combination not found.");

        existing.Name = model.Name;
        existing.GradeId = model.GradeId;
        existing.CombinationType = model.CombinationType;

        // Clear the old subject links and add the new
        existing.Subjects?.Clear();
        foreach (var subj in selectedSubjects)
        {
            existing.Subjects?.Add(subj);
        }

        // EF tracks changes. Just Save
        await _context.SaveChangesAsync();
    }
}
