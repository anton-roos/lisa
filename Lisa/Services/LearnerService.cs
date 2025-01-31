using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    public event Action? LearnersUpdated;

    public async Task<int> GetCountAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners.CountAsync();
    }

    public async Task<Learner?> GetByIdAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
        .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc.Grade)
        .Include(l => l.Combination)
            .ThenInclude(c => c.Subjects)
        .Include(l => l.LearnerSubjects)
            .ThenInclude(ls => ls.Subject)
        .Include(l => l.CareGroup)
        .Include(l => l.Parents)
        .Include(l => l.School)
        .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task AddLearnerAsync(LearnerViewModel model, List<ParentViewModel> parents, Guid schoolId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var newLearnerId = Guid.NewGuid();
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
            LearnerSubjects = [.. model.SubjectIds.Select(sid => new LearnerSubject
            {
                LearnerId = newLearnerId,
                SubjectId = sid
            })]
        };

        context.Learners.Add(learner);

        foreach (var pm in parents)
        {
            var parent = new Parent
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
            };
            context.Parents.Add(parent);
        }

        await context.SaveChangesAsync();

        await UpdateLearnerSubjectsAsync(newLearnerId, model.SubjectIds, context);
    }

    public async Task UpdateLearnerAsync(LearnerViewModel model, List<ParentViewModel> parents)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var existing = await context.Learners
            .Include(l => l.Parents)
            .FirstOrDefaultAsync(l => l.Id == model.Id);

        if (existing == null)
            throw new InvalidOperationException("Learner not found.");

        // Update Learner properties
        existing.Active = model.Active;
        existing.CareGroupId = model.CareGroupId;
        existing.CellNumber = model.CellNumber;
        existing.Code = model.Code;
        existing.Email = model.Email;
        existing.Surname = model.Surname;
        existing.IdNumber = model.IdNumber;
        existing.Name = model.Name;
        existing.RegisterClassId = model.RegisterClassId;

        // Handle Parents
        var existingParentIds = existing.Parents.Select(p => p.Id).ToList();
        var updatedParentIds = parents.Select(p => p.Id).Where(id => id.HasValue).Select(id => id.Value).ToList();

        var toRemove = existing.Parents
            .Where(p => !updatedParentIds.Contains(p.Id))
            .ToList();

        foreach (var p in toRemove)
        {
            existing.Parents.Remove(p);
            context.Parents.Remove(p);
            Console.WriteLine($"Marking Parent {p.Id} for removal.");
        }

        foreach (var pm in parents)
        {
            if (pm.Id.HasValue && existingParentIds.Contains(pm.Id.Value))
            {
                // Existing parent, update properties
                var parent = existing.Parents.FirstOrDefault(x => x.Id == pm.Id.Value);
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
                    Console.WriteLine($"Updating existing Parent {parent.Id}.");
                }
            }
            else
            {
                // New parent, add to context
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

                context.Parents.Add(newParent);
                existing.Parents.Add(newParent);
                Console.WriteLine($"Adding new Parent with Id: {newParent.Id}");
            }
        }

        // Log entity states before saving
        Console.WriteLine("ChangeTracker Entries before SaveChangesAsync:");
        foreach (var entry in context.ChangeTracker.Entries())
        {
            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, Id: {GetEntityId(entry.Entity)}, State: {entry.State}");
        }

        // First Save: Learner and Parents
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log detailed information
            Console.WriteLine($"Concurrency Exception during Learner/Parents update: {ex.Message}");

            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is Learner)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    Console.WriteLine("Conflict detected on Learner:");
                    // Log specific values if needed
                }
                else if (entry.Entity is Parent)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    Console.WriteLine($"Conflict detected on Parent Id {((Parent)entry.Entity).Id}:");
                    // Log specific values if needed
                }
            }

            throw;
        }

        // Log entity states after first SaveChangesAsync
        Console.WriteLine("ChangeTracker Entries after first SaveChangesAsync:");
        foreach (var entry in context.ChangeTracker.Entries())
        {
            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, Id: {GetEntityId(entry.Entity)}, State: {entry.State}");
        }

        // Second Save: LearnerSubjects
        try
        {
            await UpdateLearnerSubjectsAsync(existing.Id, model.SubjectIds, context);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log detailed information
            Console.WriteLine($"Concurrency Exception during LearnerSubjects update: {ex.Message}");
            throw;
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
        await using var context = await _dbContextFactory.CreateDbContextAsync();
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

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Parents.Add(newParent);
        await context.SaveChangesAsync();

        learner.Parents ??= [];
        learner.Parents.Add(newParent);
        await context.SaveChangesAsync();
    }

    public async Task<List<Learner>> GetLearnersBySchoolAsync(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
        .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.Grade)
        .Include(l => l.LearnerSubjects)
            .ThenInclude(ls => ls.Subject)
        .Include(l => l.CareGroup)
        .Include(l => l.Parents)
        .Where(l => l.SchoolId == schoolId)
        .ToListAsync();
    }

    public async Task<List<Learner>> GetLearnersBySchoolWithParentsAsync(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
        .Include(l => l.Parents)
        .Where(l => l.SchoolId == schoolId && l.Parents!.Any())
        .ToListAsync();
    }

    public async Task<List<RegisterClass>> GetRegisterClasses(School? SelectedSchool)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.RegisterClasses
            .Where(rc => rc.Grade != null && rc.Grade.SchoolId == SelectedSchool!.Id)
            .Include(rc => rc.Grade!)
            .ToListAsync();
    }



    public async Task<List<CareGroup>> GetCareGroups(School? SelectedSchool)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .Where(cg => cg.SchoolId == SelectedSchool!.Id)
            .ToListAsync();
    }

    public async Task<List<CareGroup>> GetCareGroupsBySchoolId(Guid schoolId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.CareGroups
            .Where(cg => cg.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task AddLearner(Learner learner)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Learners.Add(learner);
        await context.SaveChangesAsync();
        LearnersUpdated?.Invoke();
    }

    public async Task<List<Learner>> GetLearnersByGradeAsync(Guid gradeId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var learners = await context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.GradeId == gradeId)
            .Include(l => l.RegisterClass!)
            .ToListAsync();
        return learners;
    }

    public async Task<List<RegisterClass>> GetRegisterClassesBySchoolId(Guid schoolId)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.RegisterClasses
                            .Where(rc => rc.Grade!.SchoolId == schoolId)
                            .ToListAsync();
    }

    public async Task<List<Learner>> GetLearnersWithTheirSubjectsByGradeAsync(Guid gradeId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var learners = await context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.GradeId == gradeId)
            .Include(l => l.LearnerSubjects)
                .ThenInclude(ls => ls.Subject)
            .Include(l => l.RegisterClass!)
                .ThenInclude(rc => rc.CompulsorySubjects)
            .Include(l => l.Combination!)
                .ThenInclude(c => c.Subjects!)
            .ToListAsync();

        return learners;
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

    public async Task DeleteLearner(Learner learner)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Learners.Remove(learner);
        await context.SaveChangesAsync();
        LearnersUpdated?.Invoke();
    }
}
