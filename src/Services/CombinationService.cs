using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class CombinationService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    public async Task<Combination?> GetByIdAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Include(sc => sc.Subjects)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.School)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }

    public async Task<List<Combination>> GetCombinationsBySchoolId(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Where(c => c.SchoolGrade!.SchoolId == schoolId && !c.IsArchived)
            .Include(c => c.Subjects)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.SystemGrade)
            .ToListAsync();
    }

    public async Task<IEnumerable<Combination>> GetSubjectCombinationsForSchool(School school)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Combinations
            .AsNoTracking()
            .Include(sc => sc.Subjects)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.School)
            .Include(sc => sc.SchoolGrade)
            .ThenInclude(g => g!.SystemGrade)
            .Where(sc => sc.SchoolGrade!.SchoolId == school.Id && !sc.IsArchived)
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(Guid combinationId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var combination = await context.Combinations.FindAsync(combinationId);
        if (combination == null)
        {
            return false;
        }

        combination.IsDeleted = true;
        combination.DeletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid combinationId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var combination = await context.Combinations.FindAsync(combinationId);
        if (combination == null)
        {
            return false;
        }

        combination.IsDeleted = false;
        combination.DeletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task AddCombinationAsync(CombinationViewModel model, IEnumerable<Subject> selectedSubjects)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var newCombination = new Combination
        {
            Name = model.Name,
            SchoolGradeId = model.GradeId,
            CombinationType = model.CombinationType,
            Subjects = []
        };

        foreach (var subject in selectedSubjects)
        {
            var trackedSubject = await context.Subjects
                .FindAsync(subject.Id)
            ?? context.Subjects
                .Attach(subject).Entity;

            newCombination.Subjects.Add(trackedSubject);
        }

        await context.Combinations.AddAsync(newCombination);
        await context.SaveChangesAsync();
    }

    public async Task UpdateCombinationAsync(CombinationViewModel model, List<Subject> selectedSubjects)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var existingCombination = await context.Combinations
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == model.Id)
        ?? throw new KeyNotFoundException($"Combination with ID {model.Id} not found.");
        
        if (existingCombination.IsArchived)
        {
            throw new InvalidOperationException("Cannot edit archived combinations.");
        }

        existingCombination.Name = model.Name;
        existingCombination.SchoolGradeId = model.GradeId;
        existingCombination.CombinationType = model.CombinationType;

        var currentSubjectIds = existingCombination.Subjects?
            .Select(s => s.Id)
            .ToList() ?? [];

        var newSubjectIds = selectedSubjects
            .Select(s => s.Id)
            .ToList();


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

        foreach (var subject in selectedSubjects)
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
