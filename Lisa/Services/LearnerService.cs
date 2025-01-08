using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService(IDbContextFactory<LisaDbContext> dbContextFactory, SchoolService schoolService, IServiceProvider serviceProvider)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly SchoolService _schoolService = schoolService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public event Action? LearnersUpdated;
    public School? SelectedSchool => _schoolService.SelectedSchool;

    public async Task<Learner?> GetLearnerAsync(Guid id)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.Learners.FindAsync(id);
    }

    public async Task<Learner?> GetLearnerWithParentsAsync(Guid id)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.Learners
            .Include(l => l.LearnerParents)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Learner>> GetLearnersBySchoolAsync(Guid schoolId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.Learners
            .Where(l => l.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<List<RegisterClass>> GetRegisterClasses()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.RegisterClasses
            .Where(rc => rc.Grade != null && rc.Grade.SchoolId == SelectedSchool!.Id)
            .Include(rc => rc.Grade!)
            .ToListAsync();
    }



    public async Task<List<CareGroup>> GetCareGroups()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.CareGroups
            .Where(cg => cg.SchoolId == SelectedSchool!.Id)
            .ToListAsync();
    }

    public async Task AddLearner(Learner learner)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Learners.Add(learner);
        await _context.SaveChangesAsync();
        LearnersUpdated?.Invoke();
    }

    public async Task<List<Learner>> GetLearnersByGradeAsync(Guid gradeId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        var learners = await dbContext.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.GradeId == gradeId)
            .Include(l => l.RegisterClass!)
            .ToListAsync();
        return learners;
    }

    public async Task<Learner?> LoadLearnerAsync(Guid learnerId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Learners
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc.Grade)
            .Include(l => l.LearnerParents)
            .Include(l => l.Results)
            .ThenInclude(r => r.Subject)
            .FirstOrDefaultAsync(l => l.Id == learnerId);
    }
}