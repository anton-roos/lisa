using System.Globalization;
using Lisa.Data;
using Lisa.Models.EmailModels;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ProgressFeedbackService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<LearnerService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<LearnerService> _logger = logger;
    public async Task<ProgressFeedback?> GetProgressFeedbackAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var learner = await context.Learners
            .AsNoTracking()
            .Include(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                    .ThenInclude(sg => sg!.SystemGrade!)
            .Include(l => l.Combination!)
                .ThenInclude(c => c!.Subjects!)
            .Include(l => l.LearnerSubjects!)
                .ThenInclude(ls => ls.Subject!)
            .Include(l => l.CareGroup!)
            .Include(l => l.Parents!)
            .Include(l => l.School!)
            .Include(l => l.Results!)
                .ThenInclude(r => r.ResultSet)
                    .ThenInclude(rs => rs!.Subject)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (learner == null)
        {
            throw new Exception("No learner found.");
        }

        // Get the subjects the learner is associated with (if any)
        var subjects = learner.LearnerSubjects?
            .Select(ls => ls.Subject)
            .Where(s => s != null)
            .Distinct()
            .ToList() ?? new List<Subject>();

        var resultsBySubject = new Dictionary<string, List<Result>>();

        // For each subject, filter learner.Results where the ResultSet's Subject matches.
        foreach (var subject in subjects)
        {
            // Order descending by UpdatedAt (newest first), then take up to 6 results.
            var subjectResults = learner.Results!
                .Where(r => r.ResultSet != null &&
                            r.ResultSet.Subject != null &&
                            r.ResultSet.Subject.Id == subject.Id)
                .OrderByDescending(r => r.UpdatedAt)
                .Take(6)
                .ToList();

            resultsBySubject[subject.Name!] = subjectResults;
        }

        string learnerName = $"{learner.Name} {learner.Surname}";
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

        string learnerNamTitleCase = textInfo.ToTitleCase(learnerName.ToLower());

        var progressFeedback = new ProgressFeedback
        {
            LearnerName = learnerNamTitleCase,
            ResultsBySubject = resultsBySubject
        };

        return progressFeedback;
    }

    public async Task<List<ProgressFeedbackListItem>> GetProgressFeedbackListAsync(Guid schoolId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var list = await context.Learners
            .AsNoTracking()
            .Where(l => l.Results!.Any() && l.SchoolId == schoolId)
            .OrderBy(l => l.Name)
            .Select(l => new ProgressFeedbackListItem
            {
                LearnerId = l.Id,
                Surname = l.Surname!,
                Name = l.Name!
            })
            .ToListAsync();

        return list;
    }

    public async Task<List<ProgressFeedbackListItem>> GetProgressFeedbackListAsync
    (
        Guid schoolId, Guid? gradeId, int? subjectId
    )
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Start with the base query for learners in the selected school that have results.
        var query = context.Learners
            .AsNoTracking()
            .Where(l => l.Results!.Any() && l.SchoolId == schoolId);

        // Filter by grade if provided.
        if (gradeId is not null)
        {
            query = query.Where(l => l.RegisterClass!.SchoolGradeId == gradeId);
        }

        // Filter by subject if provided.s
        // Adjust the predicate according to your data model.
        if (subjectId is not null)
        {
            query = query.Where(l => l.LearnerSubjects!.Any(s => s.SubjectId == subjectId));
        }

        // Order the results by surname, then select the desired fields.
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

    public async Task<List<ProgressFeedbackListItem>> GetProgressFeedbackListAsync
    (
        Guid schoolId, Guid? gradeId, int? subjectId, DateTime? fromDate, DateTime? toDate
    )
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Start with the base query for learners in the selected school that have results.
        var query = context.Learners
            .AsNoTracking()
            .Where(l => l.Results!.Any() && l.SchoolId == schoolId);

        // Filter by grade if provided.
        if (gradeId is not null)
        {
            query = query.Where(l => l.RegisterClass!.SchoolGradeId == gradeId);
        }

        // Filter by subject if provided.
        if (subjectId is not null)
        {
            query = query.Where(l => l.LearnerSubjects!.Any(s => s.SubjectId == subjectId));
        }

        // Filter by date range if provided.
        if (fromDate is not null)
        {
            var fromDateUtc = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(l => l.Results!.Any(r => r.UpdatedAt >= fromDateUtc));
        }

        if (toDate is not null)
        {
            var toDateUtc = DateTime.SpecifyKind(toDate.Value.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
            query = query.Where(l => l.Results!.Any(r => r.UpdatedAt <= toDateUtc));
        }

        // Order the results by surname, then select the desired fields.
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

    public async Task<ProgressFeedback?> GetProgressFeedbackAsync(Guid id, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var learner = await context.Learners
            .AsNoTracking()
            .Include(l => l.RegisterClass!)
                .ThenInclude(rc => rc.SchoolGrade!)
                    .ThenInclude(sg => sg!.SystemGrade!)
            .Include(l => l.Combination!)
                .ThenInclude(c => c!.Subjects!)
            .Include(l => l.LearnerSubjects!)
                .ThenInclude(ls => ls.Subject!)
            .Include(l => l.CareGroup!)
            .Include(l => l.Parents!)
            .Include(l => l.School!)
            .Include(l => l.Results!)
                .ThenInclude(r => r.ResultSet)
                    .ThenInclude(rs => rs!.Subject)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (learner == null)
        {
            throw new Exception("No learner found.");
        }

        var subjects = learner.LearnerSubjects?
            .Select(ls => ls.Subject)
            .Where(s => s != null)
            .Distinct()
            .ToList() ?? new List<Subject>();

        var resultsBySubject = new Dictionary<string, List<Result>>();

        foreach (var subject in subjects)
        {
            var query = learner.Results!
                .Where(r => r.ResultSet != null &&
                           r.ResultSet.Subject != null &&
                           r.ResultSet.Subject.Id == subject.Id);

            if (fromDate.HasValue)
            {
                // Convert to UTC to ensure compatibility with PostgreSQL timestamp with time zone
                var fromDateUtc = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
                query = query.Where(r => r.UpdatedAt >= fromDateUtc);
            }

            if (toDate.HasValue)
            {
                var endDate = DateTime.SpecifyKind(toDate.Value.AddDays(1), DateTimeKind.Utc);
                query = query.Where(r => r.UpdatedAt < endDate);
            }

            var subjectResults = query
                .OrderByDescending(r => r.UpdatedAt)
                .Take(6)
                .ToList();

            resultsBySubject[subject.Name!] = subjectResults;
        }

        string learnerName = $"{learner.Name} {learner.Surname}";
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

        string learnerNamTitleCase = textInfo.ToTitleCase(learnerName.ToLower());

        var progressFeedback = new ProgressFeedback
        {
            LearnerName = learnerNamTitleCase,
            ResultsBySubject = resultsBySubject
        };

        return progressFeedback;
    }
}
