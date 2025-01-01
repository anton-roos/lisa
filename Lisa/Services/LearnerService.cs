using Lisa.Data;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService(LisaDbContext context)
{
    private readonly LisaDbContext _context = context;

    public async Task<Learner?> GetLearnerAsync(Guid id) => await _context.Learners.FindAsync(id);

    public async Task<List<Learner>> GetLearnersBySchoolAsync(Guid schoolId)
    {
        return await _context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.Grade != null && l.RegisterClass.Grade.SchoolId == schoolId)
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc.Grade!)
            .ToListAsync();
    }
}