using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ResultService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<ResultService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<ResultService> _logger = logger;

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
                Status = ResultSetStatus.Submitted,
                Results = [.. viewModel.LearnerResults.Select(entry => new Result
                {
                    Id = Guid.NewGuid(),
                    LearnerId = entry.LearnerId,
                    Score = entry.ResultViewModel.Score,
                    Absent = entry.ResultViewModel.Absent,
                    AbsentReason = entry.ResultViewModel.AbsentReason
                })]
            };

            if (viewModel.Teacher is not null)
            {
                resultSet.TeacherId = viewModel.Teacher.Id;
            }

            if (viewModel.SchoolGrade is not null)
            {
                resultSet.SchoolGradeId = viewModel.SchoolGrade.Id;
            }

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
    public async Task<int> GetCountAsync(Guid schoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Results
            .Where(x => x.ResultSet != null && x.ResultSet.SchoolGrade != null && x.ResultSet.SchoolGrade.SchoolId == schoolId)
            .CountAsync();
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

            existingResultSet.AssessmentDate = DateTime.SpecifyKind(viewModel.AssessmentDate!.Value, DateTimeKind.Utc);
            existingResultSet.AssessmentType = viewModel.AssessmentType;
            existingResultSet.AssessmentTopic = viewModel.AssessmentTopic;
            existingResultSet.UpdatedAt = DateTime.UtcNow;

            var existingResults = existingResultSet.Results?.ToDictionary(r => r.LearnerId) ?? [];

            foreach (var entry in viewModel.LearnerResults)
            {
                if (existingResults.TryGetValue(entry.LearnerId, out var result))
                {
                    result.Score = entry.ResultViewModel.Score;
                    result.Absent = entry.ResultViewModel.Absent;
                    result.AbsentReason = entry.ResultViewModel.AbsentReason;
                    result.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
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
                .Include(rs => rs.Teacher)
                .Include(rs => rs.Results!)
                .ThenInclude(r => r.Learner!)
                .ThenInclude(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade!)
                .Include(rs => rs.Subject)
                .Include(rs => rs.SchoolGrade)
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

            var query = context.ResultSets
                .AsNoTracking()
                .Include(rs => (IEnumerable<Result>)rs.Results!)
                .ThenInclude(r => r.Learner!)
                .ThenInclude(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade!)
                .Include(rs => rs.Subject)
                .Include(rs => rs.SchoolGrade)
                .Include(rs => rs.Teacher)
                .Where(rs => rs.Results != null && rs.Results.Any(r =>
                    r.Learner != null &&
                    r.Learner.RegisterClass != null &&
                    r.Learner.RegisterClass.SchoolGrade != null &&
                    r.Learner.RegisterClass.SchoolGrade.SchoolId == schoolId));

            if (gradeId.HasValue)
            {
                query = query.Where(rs => rs.SchoolGradeId == gradeId);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(rs => rs.SubjectId == subjectId.Value);
            }

            if (teacherId.HasValue)
            {
                query = query.Where(rs => rs.TeacherId == teacherId.Value);
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
}
