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
            FirstName = model.FirstName,
            IdNumber = model.IdNumber,
            LastName = model.LastName,
            RegisterClassId = model.RegisterClassId,
            SchoolId = schoolId
        };

        context.Learners.Add(learner);

        foreach (var pm in parents)
        {
            var parent = new Parent
            {
                Id = Guid.NewGuid(),
                LearnerId = newLearnerId,
                FirstName = pm.FirstName,
                LastName = pm.LastName,
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

        existing.Active = model.Active;
        existing.CareGroupId = model.CareGroupId;
        existing.CellNumber = model.CellNumber;
        existing.Code = model.Code;
        existing.Email = model.Email;
        existing.FirstName = model.FirstName;
        existing.IdNumber = model.IdNumber;
        existing.LastName = model.LastName;
        existing.RegisterClassId = model.RegisterClassId;

        var existingParentIds = existing.Parents.Select(p => p.Id).ToList();
        var updatedParentIds = parents.Select(p => p.Id).ToList();

        var toRemove = existing.Parents.Where(p => !updatedParentIds.Contains(p.Id)).ToList();
        foreach (var p in toRemove)
        {
            existing.Parents.Remove(p);
            context.Parents.Remove(p);
        }

        foreach (var pm in parents)
        {
            var parent = existing.Parents.FirstOrDefault(x => x.Id == pm.Id);
            if (parent == null)
            {
                parent = new Parent
                {
                    Id = Guid.NewGuid(),
                    LearnerId = existing.Id
                };
                existing.Parents.Add(parent);
            }

            parent.FirstName = pm.FirstName;
            parent.LastName = pm.LastName;
            parent.Relationship = pm.Relationship;
            parent.PrimaryEmail = pm.PrimaryEmail;
            parent.SecondaryEmail = pm.SecondaryEmail;
            parent.PrimaryCellNumber = pm.PrimaryCellNumber;
            parent.SecondaryCellNumber = pm.SecondaryCellNumber;
            parent.WhatsAppNumber = pm.WhatsAppNumber;
        }

        await context.SaveChangesAsync();

        await UpdateLearnerSubjectsAsync(existing.Id, model.SubjectIds, context);
    }

    public async Task UpdateLearnerSubjectsAsync(Guid learnerId, List<Guid> subjectIds, LisaDbContext context)
    {
        var existingLinks = context.LearnerSubjects
            .Where(ls => ls.LearnerId == learnerId);
        context.LearnerSubjects.RemoveRange(existingLinks);

        foreach (var sid in subjectIds)
        {
            var link = new LearnerSubject
            {
                LearnerId = learnerId,
                SubjectId = sid
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
            FirstName = parent.FirstName,
            LastName = parent.LastName,
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
            .Where(l => l.SchoolId == schoolId)
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
            .Include(l => l.RegisterClass!)
                .ThenInclude(rc => rc.CompulsorySubjects)
            .Include(l => l.Combination!)
                .ThenInclude(c => c.Subjects!)
            .ToListAsync();

        return learners;
    }

    public async Task<List<Guid>> GetSubjectIdsForLearnerAsync(Guid learnerId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return await context.LearnerSubjects
            .Where(ls => ls.LearnerId == learnerId)
            .Select(ls => ls.SubjectId)
            .ToListAsync();
    }

    public async Task AssignSubjectToLearner(Guid learnerId, Guid subjectId)
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

    public async Task RemoveSubjectFromLearner(Guid learnerId, Guid subjectId)
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
}
