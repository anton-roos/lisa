using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService(LisaDbContext context, SchoolService schoolService, IServiceProvider serviceProvider)
{
    private readonly LisaDbContext _context = context;
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

    public async Task<List<SubjectCombination>> GetSubjectCombinations()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.SubjectCombinations
            .Where(sc => sc.Grade != null && sc.Grade.SchoolId == SelectedSchool!.Id)
            .Include(sc => sc.Grade!)
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
        _context.Learners.Add(learner);
        await _context.SaveChangesAsync();
        LearnersUpdated?.Invoke();
    }
}