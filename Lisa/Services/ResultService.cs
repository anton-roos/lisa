using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ResultService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<ResultService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<ResultService> _logger = logger;

    /// <summary>
    /// Creates a new result entry.
    /// </summary>
    public async Task<bool> CreateAsync(Result result)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.Results.AddAsync(result);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created Result for LearnerId: {LearnerId}, SubjectId: {SubjectId}",
                result.LearnerId, result.SubjectId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating result for LearnerId: {LearnerId}, SubjectId: {SubjectId}",
                result.LearnerId, result.SubjectId);
            return false;
        }
    }

    /// <summary>
    /// Gets the total count of results.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Results.CountAsync();
    }

    /// <summary>
    /// Retrieves a result by ID, including related data.
    /// </summary>
    public async Task<Result?> GetByIdAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Results
                .AsNoTracking()
                .Include(r => r.Learner!)
                .ThenInclude(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade)
                .Include(r => r.Subject!)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Result with ID: {ResultId}", id);
            return null;
        }
    }

    /// <summary>
    /// Retrieves all results.
    /// </summary>
    public async Task<List<Result>> GetAllAsync()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Results
                .AsNoTracking()
                .Include(r => r.Learner!)
                .Include(r => r.Subject!)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all results.");
            return new List<Result>();
        }
    }

    /// <summary>
    /// Retrieves results based on filters.
    /// </summary>
    public async Task<List<Result>> GetResultsByFiltersAsync(Guid schoolId, Guid gradeId, int subjectId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Results
                .AsNoTracking()
                .Include(r => r.Learner!)
                .ThenInclude(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade!)
                .Include(r => r.Subject!)
                .Where(r => r.SubjectId == subjectId &&
                            r.Learner!.RegisterClass != null &&
                            r.Learner.RegisterClass.SchoolGradeId == gradeId &&
                            r.Learner.RegisterClass.SchoolGrade!.SchoolId == schoolId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching results for SchoolId: {SchoolId}, GradeId: {GradeId}, SubjectId: {SubjectId}", schoolId,
                gradeId, subjectId);
            return new List<Result>();
        }
    }

    /// <summary>
    /// Updates an existing result.
    /// </summary>
    public async Task<bool> UpdateAsync(Result result)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingResult = await context.Results.FindAsync(result.Id);
            if (existingResult == null)
            {
                _logger.LogWarning("Result with ID {ResultId} not found.", result.Id);
                return false;
            }

            existingResult.Score = result.Score;
            existingResult.Absent = result.Absent;
            existingResult.AbsentReason = result.AbsentReason;
            existingResult.UpdatedAt = DateTime.UtcNow;
            existingResult.AssessmentTopic = result.AssessmentTopic;
            existingResult.AssessmentType = result.AssessmentType;

            context.Entry(existingResult).State = EntityState.Modified;
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated Result for LearnerId: {LearnerId}, SubjectId: {SubjectId}",
                result.LearnerId, result.SubjectId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating result for LearnerId: {LearnerId}, SubjectId: {SubjectId}",
                result.LearnerId, result.SubjectId);
            return false;
        }
    }

    /// <summary>
    /// Deletes a result.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var result = await context.Results.FindAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Result with ID {ResultId} not found.", id);
                return false;
            }

            context.Results.Remove(result);
            await context.SaveChangesAsync();
            _logger.LogInformation("Deleted Result with ID {ResultId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Result with ID: {ResultId}", id);
            return false;
        }
    }
}
