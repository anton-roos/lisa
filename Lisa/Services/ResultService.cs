using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
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
    /// Creates a new ResultSet entry along with its associated Results.
    /// </summary>
    public async Task<ResultSet> CreateAsync(ResultsCaptureViewModel viewModel, Guid capturedById)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var resultSet = new ResultSet
            {
                Id = Guid.NewGuid(),
                AssessmentDate = DateTime.SpecifyKind(viewModel.AssessmentDate!.Value, DateTimeKind.Utc),
                AssessmentType = viewModel.AssessmentType,
                AssessmentTopic = viewModel.AssessmentTopic,
                SubjectId = int.Parse(viewModel.SubjectId!),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CapturedById = capturedById,
                Results = viewModel.LearnerResults.Select(entry => new Result
                {
                    Id = Guid.NewGuid(),
                    LearnerId = entry.LearnerId,
                    Score = entry.ResultViewModel.Score,
                    Absent = entry.ResultViewModel.Absent,
                    AbsentReason = entry.ResultViewModel.AbsentReason
                }).ToList()
            };

            await context.ResultSets.AddAsync(resultSet);
            await context.SaveChangesAsync();

            _logger.LogInformation("Created ResultSet for SubjectId: {SubjectId}, CapturedById: {CapturedById} with {ResultCount} results",
                resultSet.SubjectId, resultSet.CapturedById, resultSet.Results?.Count ?? 0);

            return resultSet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ResultSet for SubjectId: {SubjectId}, CapturedById: {CapturedById}",
                viewModel.SubjectId, capturedById);
            throw;
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
    /// Updates an existing ResultSet and its associated results.
    /// </summary>
    public async Task<bool> UpdateAsync(Guid resultSetId, ResultsCaptureViewModel viewModel)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingResultSet = await context.ResultSets
                .Include(rs => rs.Results)
                .FirstOrDefaultAsync(rs => rs.Id == resultSetId);

            if (existingResultSet == null)
            {
                _logger.LogWarning("ResultSet with ID {ResultSetId} not found.", resultSetId);
                return false;
            }

            // Update the result set details
            existingResultSet.AssessmentDate = DateTime.SpecifyKind(viewModel.AssessmentDate!.Value, DateTimeKind.Utc);
            existingResultSet.AssessmentType = viewModel.AssessmentType;
            existingResultSet.AssessmentTopic = viewModel.AssessmentTopic;
            existingResultSet.UpdatedAt = DateTime.UtcNow;

            // Update existing results and add new ones if necessary
            var existingResults = existingResultSet.Results.ToDictionary(r => r.LearnerId);

            foreach (var entry in viewModel.LearnerResults)
            {
                if (existingResults.TryGetValue(entry.LearnerId, out var result))
                {
                    // Update existing result
                    result.Score = entry.ResultViewModel.Score;
                    result.Absent = entry.ResultViewModel.Absent;
                    result.AbsentReason = entry.ResultViewModel.AbsentReason;
                    result.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new result if learner didn't have one before
                    var newResult = new Result
                    {
                        Id = Guid.NewGuid(),
                        LearnerId = entry.LearnerId,
                        Score = entry.ResultViewModel.Score,
                        Absent = entry.ResultViewModel.Absent,
                        AbsentReason = entry.ResultViewModel.AbsentReason,
                        ResultSetId = resultSetId
                    };
                    context.Results.Add(newResult);
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Updated ResultSet {ResultSetId} with {ResultCount} results.", resultSetId, viewModel.LearnerResults.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ResultSet with ID: {ResultSetId}", resultSetId);
            return false;
        }
    }

    /// <summary>
    /// Retrieves a result set by ID, including related data.
    /// </summary>
    public async Task<ResultSet?> GetByIdAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.ResultSets
                .AsNoTracking()
                .Include(rs => rs.Results)
                    .ThenInclude(r => r.Learner)
                        .ThenInclude(l => l.RegisterClass)
                            .ThenInclude(rc => rc.SchoolGrade)
                                .ThenInclude(sg => sg.SystemGrade)
                .Include(rs => rs.Subject)
                .FirstOrDefaultAsync(rs => rs.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ResultSet with ID: {ResultSetId}", id);
            return null;
        }
    }

    /// <summary>
    /// Retrieves results based on filters.
    /// </summary>
    public async Task<List<ResultSet>> GetResultsByFiltersAsync(
    Guid schoolId,
    Guid? gradeId,
    int? subjectId,
    Guid? teacherId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Start with the base query filtering by school.
            // Since ResultSet does not directly contain a Learner,
            // we ensure that at least one result in the set has a Learner whose class's school grade belongs to the specified school.
            var query = context.ResultSets
                .AsNoTracking()
                .Include(rs => rs.Results)
                    .ThenInclude(r => r.Learner)
                        .ThenInclude(l => l.RegisterClass)
                            .ThenInclude(rc => rc.SchoolGrade)
                                .ThenInclude(sg => sg.SystemGrade)
                .Include(rs => rs.Subject)
                .Where(rs => rs.Results.Any(r =>
                    r.Learner != null &&
                    r.Learner.RegisterClass != null &&
                    r.Learner.RegisterClass.SchoolGrade != null &&
                    r.Learner.RegisterClass.SchoolGrade.SchoolId == schoolId));

            // Apply the grade filter if provided.
            // This assumes that all results within a result set belong to the same grade.
            if (gradeId.HasValue)
            {
                query = query.Where(rs => rs.Results.Any(r =>
                    r.Learner.RegisterClass.SchoolGradeId == gradeId.Value));
            }

            // Apply the subject filter if provided.
            // Here we assume the ResultSet entity has a SubjectId property.
            if (subjectId.HasValue)
            {
                query = query.Where(rs => rs.SubjectId == subjectId.Value);
            }

            // Apply the teacher filter if provided.
            // This example assumes that the ResultSet entity records the teacher who captured the results 
            // via a property such as CapturedById.
            if (teacherId.HasValue)
            {
                query = query.Where(rs => rs.CapturedById == teacherId.Value);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching result sets for SchoolId: {SchoolId}, GradeId: {GradeId}, SubjectId: {SubjectId}, TeacherId: {TeacherId}",
                schoolId, gradeId, subjectId, teacherId);
            return [];
        }
    }

    /// <summary>
    /// Deletes a ResultSet and all associated results.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid resultSetId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var resultSet = await context.ResultSets
                .Include(rs => rs.Results)
                .FirstOrDefaultAsync(rs => rs.Id == resultSetId);

            if (resultSet == null)
            {
                _logger.LogWarning("ResultSet with ID {ResultSetId} not found.", resultSetId);
                return false;
            }

            context.Results.RemoveRange(resultSet.Results);
            context.ResultSets.Remove(resultSet);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted ResultSet with ID {ResultSetId} and its associated results.", resultSetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ResultSet with ID: {ResultSetId}", resultSetId);
            return false;
        }
    }
}
