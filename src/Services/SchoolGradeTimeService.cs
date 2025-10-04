using Lisa.Data;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolGradeTimeService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<SchoolGradeTimeService> logger
)
{
    public async Task<(TimeOnly startTime, TimeOnly endTime)> GetSchoolGradeTimesForLearnerAsync(Guid learnerId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var learner = await context.Learners
                .AsNoTracking()
                .Include(l => l.RegisterClass)
                .ThenInclude(rc => rc!.SchoolGrade)
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner?.RegisterClass?.SchoolGrade != null)
            {
                var schoolGrade = learner.RegisterClass.SchoolGrade;
                var startTime = schoolGrade.StartTime ?? new TimeOnly(8, 0);
                var endTime = schoolGrade.EndTime ?? new TimeOnly(14, 0);

                logger.LogInformation("Retrieved school grade times for learner {LearnerId}: Start {StartTime}, End {EndTime}",
                    learnerId, startTime, endTime);

                return (startTime, endTime);
            }

            logger.LogWarning("Could not find school grade for learner {LearnerId}, using default times", learnerId);
            return (new TimeOnly(8, 0), new TimeOnly(14, 0));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving school grade times for learner {LearnerId}", learnerId);
            return (new TimeOnly(8, 0), new TimeOnly(14, 0));
        }
    }

    public async Task<(TimeOnly startTime, TimeOnly endTime)> GetSchoolGradeTimesForSchoolGradeAsync(Guid schoolGradeId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var schoolGrade = await context.SchoolGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(sg => sg.Id == schoolGradeId);

            if (schoolGrade != null)
            {
                var startTime = schoolGrade.StartTime ?? new TimeOnly(8, 0);
                var endTime = schoolGrade.EndTime ?? new TimeOnly(14, 0);

                logger.LogInformation("Retrieved school grade times for school grade {SchoolGradeId}: Start {StartTime}, End {EndTime}",
                    schoolGradeId, startTime, endTime);

                return (startTime, endTime);
            }

            logger.LogWarning("Could not find school grade {SchoolGradeId}, using default times", schoolGradeId);
            return (new TimeOnly(8, 0), new TimeOnly(14, 0));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving school grade times for school grade {SchoolGradeId}", schoolGradeId);
            return (new TimeOnly(8, 0), new TimeOnly(14, 0));
        }
    }

    public async Task<bool> IsCurrentTimeWithinSchoolHoursAsync(Guid learnerId)
    {
        var (startTime, endTime) = await GetSchoolGradeTimesForLearnerAsync(learnerId);
        var currentTime = TimeOnly.FromDateTime(DateTime.Now);

        return currentTime >= startTime && currentTime <= endTime;
    }

    public async Task<bool> IsEarlyLeaveAsync(Guid learnerId, TimeOnly leaveTime)
    {
        var (_, endTime) = await GetSchoolGradeTimesForLearnerAsync(learnerId);
        return leaveTime < endTime;
    }
}
