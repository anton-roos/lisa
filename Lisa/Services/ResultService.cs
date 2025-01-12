using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services
{
    public class ResultService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<ResultService> logger)
    {
        private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
        private readonly ILogger<ResultService> _logger = logger;

        public async Task CreateAsync(Result result)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            context.Results.Add(result);
            await context.SaveChangesAsync();
            _logger.LogInformation("Result created for LearnerId: {result.LearnerId}, SubjectId: {result.SubjectId}", result.LearnerId, result.SubjectId);
        }

        public async Task<Result?> GetByIdAsync(Guid id)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Results
                .Include(r => r.Learner!)
                 .ThenInclude(l => l.RegisterClass!)
                    .ThenInclude(rc => rc.Grade)
                .Include(r => r.Subject)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Result>> GetAllAsync()
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Results
                .Include(r => r.Learner)
                .Include(r => r.Subject)
                .ToListAsync();
        }

        public async Task<List<Result>> GetResultsByFiltersAsync(Guid schoolId, Guid gradeId, Guid subjectId)
        {
            await using var context = _dbContextFactory.CreateDbContext();

            var results = await context.Results
                .Include(r => r.Learner!)
                    .ThenInclude(l => l.RegisterClass!)
                        .ThenInclude(rc => rc.Grade)
                .Include(r => r.Subject!)
                .Where(r => r.SubjectId == subjectId &&
                            r.Learner!.RegisterClass != null &&
                            r.Learner.RegisterClass.GradeId == gradeId &&
                            r.Learner.RegisterClass.Grade!.SchoolId == schoolId)
                .ToListAsync();

            return results;
        }


        public async Task UpdateAsync(Result result)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var existingResult = await context.Results.FindAsync(result.Id);
            if (existingResult == null)
            {
                _logger.LogWarning($"Result with ID {result.Id} not found.");
                return;
            }

            existingResult.Score = result.Score;
            existingResult.Absent = result.Absent;
            existingResult.AbsentReason = result.AbsentReason;
            existingResult.UpdatedAt = DateTime.UtcNow;
            existingResult.AssessmentTopic = result.AssessmentTopic;
            existingResult.AssessmentType = result.AssessmentType;

            await context.SaveChangesAsync();
            _logger.LogInformation($"Result updated for LearnerId: {result.LearnerId}, SubjectId: {result.SubjectId}");
        }

        public async Task DeleteAsync(Guid id)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var result = await context.Results.FindAsync(id);
            if (result == null)
            {
                _logger.LogWarning($"Result with ID {id} not found.");
                return;
            }

            context.Results.Remove(result);
            await context.SaveChangesAsync();
            _logger.LogInformation("Result with ID {id} deleted.", id);
        }
    }
}
