using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ResultService
(
    IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<ResultService> logger
)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<ResultService> _logger = logger;

    public async Task<ResultSet> CreateAsync(ResultsCaptureViewModel viewModel, Guid capturedById)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            DateTime? assessmentDate = null;
            if (viewModel.AssessmentDate is not null)
            {
                assessmentDate = DateTime.SpecifyKind(viewModel.AssessmentDate!.Value, DateTimeKind.Utc);
            }

            var resultSet = new ResultSet
            {
                Id = Guid.NewGuid(),
                AssessmentDate = assessmentDate,
                AssessmentTypeId = viewModel.AssessmentType?.Id ?? 0,
                AssessmentTopic = viewModel.AssessmentTopic,
                SubjectId = int.Parse(viewModel.SubjectId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CapturedById = capturedById,
                Status = viewModel.Status,
                Results = viewModel.LearnerResults.Select(entry => new Result
                {
                    Id = Guid.NewGuid(),
                    LearnerId = entry.LearnerId,
                    Score = entry.ResultViewModel.Score,
                    Absent = entry.ResultViewModel.Absent,
                    AbsentReason = entry.ResultViewModel.AbsentReason
                }).ToList()
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

            if (viewModel.AssessmentDate is not null)
            {
                existingResultSet.AssessmentDate = DateTime.SpecifyKind(viewModel.AssessmentDate!.Value, DateTimeKind.Utc);
            }
            existingResultSet.AssessmentTypeId = viewModel.AssessmentType?.Id ?? 0;
            existingResultSet.AssessmentTopic = viewModel.AssessmentTopic;
            existingResultSet.UpdatedAt = DateTime.UtcNow;
            existingResultSet.Status = viewModel.Status;

            var existingResults = existingResultSet.Results?.ToDictionary(r => r.LearnerId)
                                  ?? new Dictionary<Guid, Result>();

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

    public async Task SyncLearnerResultsAsync(Guid resultSetId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var resultSet = await context.ResultSets
            .Include(rs => rs.Results)
            .FirstOrDefaultAsync(rs => rs.Id == resultSetId);

        if (resultSet == null || resultSet.SchoolGradeId == null)
            return;

        var gradeLearnerIds = await context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.SchoolGradeId == resultSet.SchoolGradeId)
            .Select(l => l.Id)
            .ToListAsync();

        var currentLearnerIds = resultSet.Results!.Select(r => r.LearnerId).ToList();
        foreach (var learnerId in gradeLearnerIds)
        {
            if (!currentLearnerIds.Contains(learnerId))
            {
                var newResult = new Result
                {
                    Id = Guid.NewGuid(),
                    LearnerId = learnerId,
                    Score = null,
                    Absent = false,
                    AbsentReason = null,
                    ResultSetId = resultSetId,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Results.Add(newResult);
            }
        }

        await context.SaveChangesAsync();
    }

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

            context.ResultSets.Remove(resultSet);
            await context.SaveChangesAsync();
            _logger.LogInformation("Deleted ResultSet {ResultSetId} with {ResultCount} results.", resultSetId, resultSet.Results?.Count ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ResultSet with ID: {ResultSetId}", resultSetId);
            return false;
        }
    }

    public async Task<ResultSet?> GetByIdAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var resultSet = await context.ResultSets
                .AsNoTracking()
                .Include(rs => rs.Teacher)
                .Include(r => r.AssessmentType)
                .Include(rs => rs.Results!)
                    .ThenInclude(r => r.Learner!)
                        .ThenInclude(l => l.RegisterClass!)
                            .ThenInclude(rc => rc.SchoolGrade!)
                                .ThenInclude(sg => sg.SystemGrade)
                .Include(rs => rs.Subject)
                .Include(rs => rs.SchoolGrade)
                .FirstOrDefaultAsync(rs => rs.Id == id);

            if (resultSet?.Results != null)
            {
                resultSet.Results = resultSet.Results
                    .OrderBy(s => s.Learner!.Surname)
                    .ThenBy(s => s.Learner!.Name)
                    .ToList();
            }

            return resultSet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ResultSet with ID: {ResultSetId}", id);
            return null;
        }
    }

    public async Task<List<ResultSet?>> GetResultsByFiltersAsync(
        Guid schoolId,
        Guid? gradeId,
        int? subjectId,
        Guid? teacherId,
        Guid? learnerId,
        DateTime? fromDate,
        DateTime? toDate
    )
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var query = context.ResultSets
                .AsNoTracking()
                .Include(r => r.AssessmentType)
                .Include(rs => rs.Results!)
                .ThenInclude(r => r.Learner!)
                .ThenInclude(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade)
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

            if (learnerId.HasValue)
            {
                query = query.Where(rs => rs.Results!.Any(r => r.LearnerId == learnerId));
            }

            if (fromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
                query = query.Where(rs => rs.AssessmentDate != null && rs.AssessmentDate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
                query = query.Where(rs => rs.AssessmentDate != null && rs.AssessmentDate <= toUtc);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching result sets for SchoolId: {SchoolId}, GradeId: {GradeId}, SubjectId: {SubjectId}, TeacherId: {TeacherId}",
                schoolId, gradeId, subjectId, teacherId);
            return new List<ResultSet?>();
        }
    }

    public async Task RemoveLearnerResultsAsync(Guid resultSetId, List<Guid> removedLearnerIds)
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
                return;
            }

            var resultsToRemove = resultSet.Results!
                .Where(r => removedLearnerIds.Contains(r.LearnerId))
                .ToList();

            if (resultsToRemove.Any())
            {
                context.Results.RemoveRange(resultsToRemove);
                await context.SaveChangesAsync();
                _logger.LogInformation("Removed {RemovedCount} learner results from ResultSet {ResultSetId}.", resultsToRemove.Count, resultSetId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing learner results from ResultSet {ResultSetId}.", resultSetId);
            throw;
        }
    }
}
