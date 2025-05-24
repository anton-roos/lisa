using System.Globalization;
using Lisa.Data;
using Lisa.Models.EmailModels;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ProgressFeedbackService
(
    LearnerService learnerService,
    IDbContextFactory<LisaDbContext> dbContextFactory
)
{
    public async Task<ProgressFeedback?> GetProgressFeedbackAsync(Guid learnerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var learner = await learnerService.GetByIdAsync(learnerId, activeOnly: true);

        Guard.Against.Null(learner, nameof(learner), "Learner not found or inactive in get progress feedback.");

        var resultsBySubject = new Dictionary<string, List<Result>>();

        var resultsBySubjectId = learner.Results?
            .Where(r => r.ResultSet?.Subject != null)
            .GroupBy(r => r.ResultSet!.Subject!.Id)
            .ToList() ?? [];

        foreach (var group in resultsBySubjectId)
        {
            var subjectResults = group.AsEnumerable();

            if (fromDate.HasValue)
            {
                subjectResults = subjectResults
                    .Where(r => r.ResultSet!.AssessmentDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                subjectResults = subjectResults
                    .Where(r => r.ResultSet!.AssessmentDate < toDate.Value);
            }

            var filteredResults = subjectResults
                .OrderByDescending(r => r.ResultSet!.AssessmentDate)
                .Take(6)
                .ToList();

            if (filteredResults.Count <= 0) continue;
            var subjectName = filteredResults.First().ResultSet!.Subject!.Name!;
            resultsBySubject[subjectName] = filteredResults;
        }

        var learnerName = $"{learner.Name} {learner.Surname}";
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        var learnerNameTitleCase = textInfo.ToTitleCase(learnerName.ToLower());

        var progressFeedback = new ProgressFeedback
        {
            LearnerName = learnerNameTitleCase,
            ResultsBySubject = resultsBySubject
        };

        return progressFeedback;
    }

    public async Task<List<ProgressFeedbackListItem>> GetProgressFeedbackListAsync(Guid schoolId, Guid? gradeId = null, int? subjectId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var query = context.Learners
            .AsNoTracking()
            .Where(l => l.Results!.Any() && l.SchoolId == schoolId && l.Active);

        if (fromDate.HasValue || toDate.HasValue)
        {
            query = query.Include(l => l.Results!)
                        .ThenInclude(r => r.ResultSet);
        }

        if (gradeId.HasValue)
        {
            query = query.Where(l => l.RegisterClass!.SchoolGradeId == gradeId);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(l => l.LearnerSubjects!.Any(s => s.SubjectId == subjectId));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.Results!.Any(r => r.ResultSet != null && r.ResultSet.AssessmentDate >= fromDate.Value));
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.Results!.Any(r => r.ResultSet != null && r.ResultSet.AssessmentDate <= toDate.Value.AddDays(1).AddSeconds(-1)));
        }

        var list = await query
            .OrderBy(l => l.Surname)
            .Select(l => new ProgressFeedbackListItem
            {
                LearnerId = l.Id,
                Surname = l.Surname!,
                Name = l.Name!
            })
            .ToListAsync();

        return list;
    }
}