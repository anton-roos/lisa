using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService(LisaDbContext context)
{
    private readonly LisaDbContext _context = context;
    public event Action? LearnersUpdated;

    public async Task<Learner?> GetLearnerAsync(Guid id) => await _context.Learners.FindAsync(id);

    public async Task<List<Learner>> GetLearnersBySchoolAsync(Guid schoolId)
    {
        return await _context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.Grade != null && l.RegisterClass.Grade.SchoolId == schoolId)
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.Grade!)
            .ToListAsync();
    }

    public async Task AddLearner(Learner learner)
    {
        _context.Learners.Add(learner);
        await _context.SaveChangesAsync();
        LearnersUpdated?.Invoke();
    }
}