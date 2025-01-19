using Lisa.Data;
using Lisa.Models.Entities;
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

    public async Task<Learner?> GetLearnerAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners.FindAsync(id);
    }

    public async Task<Learner?> GetLearnerWithParentsAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Include(l => l.Parents)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task AddParentToLearnerAsync(Parent parent)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Parents.Add(parent);
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
                .ThenInclude(c => c.Subjects)
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
}