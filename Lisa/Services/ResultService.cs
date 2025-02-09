using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ResultService
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;
    private readonly ILogger<ResultService> _logger;

    public ResultService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<ResultService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

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
            _logger.LogInformation("Created Result for LearnerId: {LearnerId}, ResultSetId: {ResultSetId}",
                result.LearnerId, result.ResultSetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating result for LearnerId: {LearnerId}, ResultSetId: {ResultSetId}",
                result.LearnerId, result.ResultSetId);
            return false;
        }
    }

    /// <summary>
    /// Creates a new ResultSet entry along with its associated Results.
    /// </summary>
    public async Task<bool> CreateResultSetAsync(ResultSet resultSet)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            // Adding the ResultSet will also add the child Results (if any) via cascading.
            await context.ResultSets.AddAsync(resultSet);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created ResultSet for SubjectId: {SubjectId}, CapturedById: {CapturedById} with {ResultCount} results",
                resultSet.SubjectId, resultSet.CapturedById, resultSet.Results?.Count ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ResultSet for SubjectId: {SubjectId}, CapturedById: {CapturedById}",
                resultSet.SubjectId, resultSet.CapturedById);
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
                .Include(r => r.Learner)
                    .ThenInclude(l => l.RegisterClass)
                        .ThenInclude(rc => rc.SchoolGrade)
                            .ThenInclude(sg => sg.SystemGrade)
                .Include(r => r.ResultSet)
                    .ThenInclude(rs => rs.Subject)
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
                .Include(r => r.Learner)
                .Include(r => r.ResultSet)
                    .ThenInclude(rs => rs.Subject)
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
                .Include(r => r.Learner)
                    .ThenInclude(l => l.RegisterClass)
                        .ThenInclude(rc => rc.SchoolGrade)
                            .ThenInclude(sg => sg.SystemGrade)
                .Include(r => r.ResultSet)
                    .ThenInclude(rs => rs.Subject)
                .Where(r => r.ResultSet.SubjectId == subjectId &&
                            r.Learner!.RegisterClass != null &&
                            r.Learner.RegisterClass.SchoolGradeId == gradeId &&
                            r.Learner.RegisterClass.SchoolGrade!.SchoolId == schoolId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching results for SchoolId: {SchoolId}, GradeId: {GradeId}, SubjectId: {SubjectId}",
                schoolId, gradeId, subjectId);
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

            // Update only the fields that are stored on the Result entity.
            existingResult.Score = result.Score;
            existingResult.Absent = result.Absent;
            existingResult.AbsentReason = result.AbsentReason;
            existingResult.UpdatedAt = DateTime.UtcNow;

            context.Entry(existingResult).State = EntityState.Modified;
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated Result for LearnerId: {LearnerId}, ResultSetId: {ResultSetId}",
                result.LearnerId, result.ResultSetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating result for LearnerId: {LearnerId}, ResultSetId: {ResultSetId}",
                result.LearnerId, result.ResultSetId);
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
