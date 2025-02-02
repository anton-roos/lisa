using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CombinationService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    /// <summary>
    /// Create a new Combination.
    /// </summary>
    public async Task<Combination> CreateAsync(Combination subjectCombination)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.Combinations.AddAsync(subjectCombination);
        await context.SaveChangesAsync();
        return subjectCombination;
    }

    /// <summary>
    /// Retrieve a Combination by ID.
    /// </summary>
    public async Task<Combination?> GetByIdAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Include(sc => sc.Subjects)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.School)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(sg => sg.SystemGrade)
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }

    /// <summary>
    /// Retrieve all Combinations.
    /// </summary>
    public async Task<IEnumerable<Combination>> GetAllAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Include(sc => sc.Subjects)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieve Combinations for a specific School ID.
    /// </summary>
    public async Task<List<Combination>> GetCombinationsBySchoolId(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Where(c => c.SchoolGrade!.SchoolId == schoolId)
            .Include(c => c.Subjects)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.SystemGrade)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieve Subject Combinations for a specific School.
    /// </summary>
    public async Task<IEnumerable<Combination>> GetSubjectCombinationsForSchool(School school)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Include(sc => sc.Subjects)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.School)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.SystemGrade)
            .Where(sc => sc.SchoolGrade!.SchoolId == school.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Delete a Combination by ID.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var subjectCombination = await context.Combinations.FindAsync(id);
        if (subjectCombination == null)
        {
            return; // No need to save changes if nothing was found.
        }

        context.Combinations.Remove(subjectCombination);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Add a new Combination.
    /// </summary>
    public async Task AddCombinationAsync(CombinationViewModel model, IEnumerable<Subject> selectedSubjects)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var newCombination = new Combination
        {
            Name = model.Name,
            SchoolGradeId = model.GradeId,
            CombinationType = model.CombinationType,
            Subjects = new List<Subject>()
        };

        foreach (var subject in selectedSubjects)
        {
            var trackedSubject = await context.Subjects.FindAsync(subject.Id)
                                 ?? context.Subjects.Attach(subject).Entity;

            newCombination.Subjects.Add(trackedSubject);
        }

        await context.Combinations.AddAsync(newCombination);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Update an existing Combination.
    /// </summary>
    public async Task UpdateCombinationAsync(CombinationViewModel model, IEnumerable<Subject> selectedSubjects)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var existingCombination = await context.Combinations
                                      .Include(c => c.Subjects)
                                      .FirstOrDefaultAsync(c => c.Id == model.Id)
                                  ?? throw new KeyNotFoundException($"Combination with ID {model.Id} not found.");

        existingCombination.Name = model.Name;
        existingCombination.SchoolGradeId = model.GradeId;
        existingCombination.CombinationType = model.CombinationType;

        var currentSubjectIds = existingCombination.Subjects?
            .Select(s => s.Id)
            .ToList() ?? [];

        var newSubjectIds = selectedSubjects?
            .Select(s => s.Id)
            .ToList() ?? [];

        var subjectsToRemove = existingCombination.Subjects?
            .Where(s => !newSubjectIds.Contains(s.Id))
            .ToList() ?? [];

        if (existingCombination.Subjects != null)
        {
            foreach (var subject in subjectsToRemove)
            {
                existingCombination.Subjects.Remove(subject);
            }
        }

        foreach (var subject in selectedSubjects ?? [])
        {
            if (!currentSubjectIds.Contains(subject.Id))
            {
                var trackedSubject = await context.Subjects.FindAsync(subject.Id)
                                     ?? context.Subjects.Attach(subject).Entity;

                existingCombination.Subjects?.Add(trackedSubject);
            }
        }

        await context.SaveChangesAsync();
    }
}