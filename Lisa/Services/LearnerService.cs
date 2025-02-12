using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<LearnerService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<LearnerService> _logger = logger;

    public event Action? LearnersUpdated;

    /// <summary>
    /// Gets the total number of learners.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners.CountAsync();
    }
    public async Task<int> GetCountAsync(Guid SchoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners.Where(x => x.SchoolId == SchoolId).CountAsync();
    }

    /// <summary>
    /// Retrieves a learner by ID, including related data.
    /// </summary>
    public async Task<Learner?> GetByIdAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .AsNoTracking()
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.Combination)
            .ThenInclude(c => c!.Subjects)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject!)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents!)
            .Include(l => l.School)

            .FirstOrDefaultAsync(l => l.Id == id);
    }

    /// <summary>
    /// Retrieves a list of Learners by Subject Id
    /// </summary>
    public async Task<List<Learner>> GetBySubjectIdAsync(int subjectId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.LearnerSubjects
            .Where(ls => ls.SubjectId == subjectId)
            .Select(ls => ls.Learner)
            .ToListAsync();
    }

    public async Task<bool> LearnerExistsByCodeAsync(string code)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners.AnyAsync(l => l.Code == code);
    }

    /// <summary>
    /// Adds a new learner.
    /// </summary>
    public async Task<bool> AddLearnerAsync(LearnerViewModel model, List<ParentViewModel> parents, Guid schoolId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var newLearnerId = Guid.NewGuid();

            // Build the collection of LearnerSubjects, including CombinationId
            var learnerSubjects = new List<LearnerSubject>();
            foreach (var sid in model.SubjectIds)
            {
                // Determine if this subjectId was chosen as part of a combination
                var combId = FindCombinationId(model.CombinationSelections, sid);

                learnerSubjects.Add(new LearnerSubject
                {
                    LearnerId = newLearnerId,
                    SubjectId = sid,
                    CombinationId = combId
                });
            }

            var learner = new Learner
            {
                Id = newLearnerId,
                Active = model.Active,
                CareGroupId = model.CareGroupId,
                CellNumber = model.CellNumber,
                Code = model.Code,
                Email = model.Email,
                Surname = model.Surname,
                IdNumber = model.IdNumber,
                Name = model.Name,
                RegisterClassId = model.RegisterClassId,
                SchoolId = schoolId,
                LearnerSubjects = learnerSubjects
            };

            await context.Learners.AddAsync(learner);

            var parentEntities = parents.Select(pm => new Parent
            {
                Id = Guid.NewGuid(),
                LearnerId = newLearnerId,
                Surname = pm.Surname,
                Name = pm.Name,
                Relationship = pm.Relationship,
                PrimaryEmail = pm.PrimaryEmail,
                SecondaryEmail = pm.SecondaryEmail,
                PrimaryCellNumber = pm.PrimaryCellNumber,
                SecondaryCellNumber = pm.SecondaryCellNumber,
                WhatsAppNumber = pm.WhatsAppNumber
            }).ToList();

            await context.Parents.AddRangeAsync(parentEntities);

            await context.SaveChangesAsync();

            LearnersUpdated?.Invoke();
            _logger.LogInformation("Added learner {LearnerId} successfully.", newLearnerId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding learner.");
            return false;
        }
    }

    /// <summary>
    /// Updates an existing learner.
    /// </summary>
    public async Task<bool> UpdateLearnerAsync(LearnerViewModel model, List<ParentViewModel> parents)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var existing = await context.Learners
                .Include(l => l.Parents)
                .Include(l => l.LearnerSubjects)
                .FirstOrDefaultAsync(l => l.Id == model.Id);

            if (existing == null)
            {
                _logger.LogWarning("Learner {LearnerId} not found for update.", model.Id);
                return false;
            }

            existing.Active = model.Active;
            existing.CareGroupId = model.CareGroupId;
            existing.CellNumber = model.CellNumber;
            existing.Code = model.Code;
            existing.Email = model.Email;
            existing.Surname = model.Surname;
            existing.IdNumber = model.IdNumber;
            existing.Name = model.Name;
            existing.RegisterClassId = model.RegisterClassId;

            var existingParentIds = existing.Parents?.Select(p => p.Id).ToList() ?? new List<Guid>();
            var updatedParentIds = parents.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).ToList();

            var parentsToRemove = existing.Parents?.Where(p => !updatedParentIds.Contains(p.Id)).ToList() ?? new List<Parent>();
            context.Parents.RemoveRange(parentsToRemove);

            foreach (var pm in parents)
            {
                if (pm.Id.HasValue && existingParentIds.Contains(pm.Id.Value))
                {
                    var parent = existing.Parents?.First(x => x.Id == pm.Id.Value);
                    if (parent != null)
                    {
                        parent.Surname = pm.Surname;
                        parent.Name = pm.Name;
                        parent.Relationship = pm.Relationship;
                        parent.PrimaryEmail = pm.PrimaryEmail;
                        parent.SecondaryEmail = pm.SecondaryEmail;
                        parent.PrimaryCellNumber = pm.PrimaryCellNumber;
                        parent.SecondaryCellNumber = pm.SecondaryCellNumber;
                        parent.WhatsAppNumber = pm.WhatsAppNumber;
                    }
                }
                else
                {
                    var newParent = new Parent
                    {
                        Id = Guid.NewGuid(),
                        LearnerId = existing.Id,
                        Surname = pm.Surname,
                        Name = pm.Name,
                        Relationship = pm.Relationship,
                        PrimaryEmail = pm.PrimaryEmail,
                        SecondaryEmail = pm.SecondaryEmail,
                        PrimaryCellNumber = pm.PrimaryCellNumber,
                        SecondaryCellNumber = pm.SecondaryCellNumber,
                        WhatsAppNumber = pm.WhatsAppNumber
                    };
                    await context.Parents.AddAsync(newParent);
                }
            }

            if (existing.LearnerSubjects != null && existing.LearnerSubjects.Any())
            {
                context.LearnerSubjects.RemoveRange(existing.LearnerSubjects);
            }

            var newLearnerSubjects = new List<LearnerSubject>();
            foreach (var sid in model.SubjectIds)
            {
                var combId = FindCombinationId(model.CombinationSelections, sid);

                newLearnerSubjects.Add(new LearnerSubject
                {
                    LearnerId = existing.Id,
                    SubjectId = sid,
                    CombinationId = combId
                });
            }

            await context.LearnerSubjects.AddRangeAsync(newLearnerSubjects);

            await context.SaveChangesAsync();

            LearnersUpdated?.Invoke();
            _logger.LogInformation("Updated learner {LearnerId} successfully.", model.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating learner.");
            return false;
        }
    }

    private static Guid GetEntityId(object entity)
    {
        return entity switch
        {
            Learner learner => learner.Id,
            Parent parent => parent.Id,
            LearnerSubject ls => ls.LearnerId,
            _ => Guid.Empty
        };
    }

    public async Task UpdateLearnerSubjectsAsync(Guid learnerId, List<int> subjectIds, LisaDbContext context)
    {
        var existingLinks = context.LearnerSubjects
            .Where(ls => ls.LearnerId == learnerId);
        context.LearnerSubjects.RemoveRange(existingLinks);

        foreach (var subjectId in subjectIds)
        {
            var link = new LearnerSubject
            {
                LearnerId = learnerId,
                SubjectId = subjectId
            };
            context.LearnerSubjects.Add(link);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Learner?> GetLearnerWithParentsAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Include(l => l.Parents)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task AddParentToLearnerAsync(ParentViewModel parent, Learner learner)
    {
        var newParent = new Parent
        {
            Surname = parent.Surname,
            Name = parent.Name,
            PrimaryEmail = parent.PrimaryEmail,
            SecondaryEmail = parent.SecondaryEmail,
            PrimaryCellNumber = parent.PrimaryCellNumber,
            SecondaryCellNumber = parent.SecondaryCellNumber,
            WhatsAppNumber = parent.WhatsAppNumber,
            Relationship = parent.Relationship,
            LearnerId = learner.Id
        };

        using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Parents.Add(newParent);
        await context.SaveChangesAsync();

        learner.Parents ??= [];
        learner.Parents.Add(newParent);
        await context.SaveChangesAsync();
    }

    public async Task<List<Learner>> GetLearnersBySchoolAsync(Guid schoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject!)
            .Include(l => l.CareGroup!)
            .Include(l => l.Parents!)
            .Where(l => l.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<List<Learner>> GetLearnersBySchoolWithParentsAsync(Guid schoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Include(l => l.Parents)
            .Where(l => l.SchoolId == schoolId && l.Parents!.Any())
            .ToListAsync();
    }


    public async Task<List<Learner>> GetLearnersByGradeAsync(Guid gradeId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var learners = await context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.SchoolGradeId == gradeId)
            .Include(l => l.RegisterClass!)
            .ThenInclude(r => r.SchoolGrade)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.LearnerSubjects)
            .ThenInclude(ls => ls.Subject!)
            .ToListAsync();
        return learners;
    }

    public async Task<List<Learner>> GetLearnersWithTheirSubjectsByGradeAsync(Guid gradeId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.SchoolGradeId == gradeId)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject!)
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.CompulsorySubjects!)
            .Include(l => l.Combination!)
            .ThenInclude(c => c.Subjects!)
            .ToListAsync();
    }

    public async Task<List<int>> GetSubjectIdsForLearnerAsync(Guid learnerId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return await context.LearnerSubjects
            .Where(ls => ls.LearnerId == learnerId)
            .Select(ls => ls.SubjectId)
            .ToListAsync();
    }

    public async Task AssignSubjectToLearner(Guid learnerId, int subjectId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var link = new LearnerSubject
        {
            LearnerId = learnerId,
            SubjectId = subjectId
        };
        context.LearnerSubjects.Add(link);
        await context.SaveChangesAsync();
    }

    public async Task RemoveSubjectFromLearner(Guid learnerId, int subjectId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var link = await context.LearnerSubjects
            .FirstOrDefaultAsync(ls => ls.LearnerId == learnerId && ls.SubjectId == subjectId);

        if (link != null)
        {
            context.LearnerSubjects.Remove(link);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Subject>> GetSubjectsForLearner(Guid learnerId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return await context.LearnerSubjects
            .Where(ls => ls.LearnerId == learnerId)
            .Select(ls => ls.Subject)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes a learner.
    /// </summary>
    public async Task<bool> DeleteLearnerAsync(Guid learnerId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var learner = await context.Learners.FindAsync(learnerId);

            if (learner == null)
            {
                _logger.LogWarning("Attempted to delete non-existent learner {LearnerId}.", learnerId);
                return false;
            }

            context.Learners.Remove(learner);
            await context.SaveChangesAsync();
            LearnersUpdated?.Invoke();
            _logger.LogInformation("Deleted learner {LearnerId}.", learnerId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting learner.");
            return false;
        }
    }

    private Guid? FindCombinationId(Dictionary<Guid, int> combinationSelections, int subjectId)
    {
        // combinationSelections is a dictionary: 
        // Key   = CombinationId
        // Value = SubjectId chosen for that combination.

        // We look for the entry where the chosen subjectId matches
        var match = combinationSelections
            .FirstOrDefault(kvp => kvp.Value == subjectId);

        // If no match was found, Key would be Guid.Empty
        if (match.Key == Guid.Empty)
            return null;

        return match.Key;
    }

}
