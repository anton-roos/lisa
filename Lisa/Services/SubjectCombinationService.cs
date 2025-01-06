using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SubjectCombinationService
{
    private readonly LisaDbContext _dbContext;

    public SubjectCombinationService(LisaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(SubjectCombination combo, List<Guid> subjectIds)
    {
        // 1. Create the SubjectCombination itself:
        var newCombo = new SubjectCombination
        {
            Name = combo.Name,
            GradeId = combo.GradeId
        };
        _dbContext.SubjectCombinations.Add(newCombo);
        await _dbContext.SaveChangesAsync();

        // 2. Create the linked subjects
        foreach (var sid in subjectIds)
        {
            var newLink = new SubjectCombinationSubject
            {
                SubjectCombinationId = newCombo.Id,
                SubjectId = sid
            };
            _dbContext.SubjectCombinationSubjects.Add(newLink);
        }

        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<SubjectCombination?> GetByIdAsync(Guid id)
    {
        return await _dbContext.SubjectCombinations
            .Include(sc => sc.SubjectCombinationSubjects)
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }

    public async Task UpdateAsync(SubjectCombination combo, List<Guid> subjectIds)
    {
        // 1. Update the SubjectCombination itself:
        var existing = await _dbContext.SubjectCombinations
            .Include(sc => sc.SubjectCombinationSubjects)
            .FirstOrDefaultAsync(sc => sc.Id == combo.Id);
        if (existing == null) return;

        existing.Name = combo.Name;
        existing.GradeId = combo.GradeId;

        // 2. Update the linked subjects
        // Remove any links not in subjectIds
        var toRemove = existing.SubjectCombinationSubjects
            .Where(link => !subjectIds.Contains(link.SubjectId))
            .ToList();
        _dbContext.SubjectCombinationSubjects.RemoveRange(toRemove);

        // Add any new links that don't exist yet
        var existingSubjectIds = existing.SubjectCombinationSubjects
            .Select(link => link.SubjectId)
            .ToList();
        var toAddIds = subjectIds.Except(existingSubjectIds);
        foreach (var sid in toAddIds)
        {
            var newLink = new SubjectCombinationSubject
            {
                SubjectCombinationId = existing.Id,
                SubjectId = sid
            };
            _dbContext.SubjectCombinationSubjects.Add(newLink);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<SubjectCombination>> GetAllAsync()
    {
        return await _dbContext.SubjectCombinations
            .Include(sc => sc.SubjectCombinationSubjects)
            .ToListAsync();
    }
}