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
        .Include(l => l.CareGroup)
        .Include(l => l.Parents)
        .Include(l => l.School)
        .FirstOrDefaultAsync(l => l.Id == id);
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


    public async Task<Learner?> LoadLearnerAsync(Guid learnerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var learner = await context.Learners
            .Include(l => l.Combination)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents)
            .Include(l => l.RegisterClass!)
                .ThenInclude(rc => rc.Grade)
            .FirstOrDefaultAsync(l => l.Id == learnerId);
        return learner;
    }

    public async Task<List<Guid>> GetSubjectIdsForLearnerAsync(Guid learnerId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var learner = await context.Learners
            .Include(l => l.Combination)
                .ThenInclude(c => c.Subjects)
            .FirstOrDefaultAsync(l => l.Id == learnerId);
        var subjectIds = new List<Guid>();
        if (learner.Combination != null)
        {
            foreach (var subject in learner.Combination.Subjects)
            {
                subjectIds.Add(subject.Id);
            }
        }
        return subjectIds;
    }

    public async Task UpdateLearnerSubjectsAsync(Guid learnerId, List<Guid> subjectIds)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var learner = await context.Learners
            .Include(l => l.Combination)
                .ThenInclude(c => c.Subjects)
            .FirstOrDefaultAsync(l => l.Id == learnerId);

        if (learner == null)
        {
            return;
        }

        learner.Combination ??= new Combination();
        learner.Combination.Subjects?.Clear();

        foreach (var subjectId in subjectIds)
        {
            var subject = await context.Subjects.FindAsync(subjectId);
            if (subject != null)
            {
                learner.Combination.Subjects ??= [];
                learner.Combination.Subjects.Add(subject);
            }
        }
        await context.SaveChangesAsync();
    }
}
