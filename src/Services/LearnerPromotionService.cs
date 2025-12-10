using System.Text.Json;
using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerPromotionService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<LearnerPromotionService> logger
)
{
    public async Task<bool> EnterYearEndModeAsync(Guid schoolId, int academicYear)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var school = await context.Schools
                .Include(s => s.Learners!)
                    .ThenInclude(l => l.LearnerSubjects!)
                        .ThenInclude(ls => ls.Subject)
                .Include(s => s.Learners!)
                    .ThenInclude(l => l.Combination)
                .Include(s => s.Learners!)
                    .ThenInclude(l => l.RegisterClass)
                .FirstOrDefaultAsync(s => s.Id == schoolId);

            if (school == null)
            {
                logger.LogWarning("School {SchoolId} not found.", schoolId);
                return false;
            }

            if (school.IsYearEndMode)
            {
                logger.LogWarning("School {SchoolId} is already in Year End Mode.", schoolId);
                return false;
            }

            var activeLearners = school.Learners?.Where(l => !l.IsDisabled && l.Status == Lisa.Enums.LearnerStatus.Active).ToList() ?? [];

            foreach (var learner in activeLearners)
            {
                // Archive current state
                var subjectSnapshot = learner.LearnerSubjects?
                    .Select(ls => new { ls.SubjectId, ls.Subject?.Name, ls.Subject?.Code })
                    .ToList();

                var historyRecord = new LearnerAcademicRecord
                {
                    Id = Guid.NewGuid(),
                    LearnerId = learner.Id,
                    Year = academicYear,
                    SchoolGradeId = learner.RegisterClass?.SchoolGradeId ?? learner.Combination?.SchoolGradeId ?? Guid.Empty,
                    RegisterClassId = learner.RegisterClassId,
                    CombinationId = learner.CombinationId,
                    SubjectSnapshot = subjectSnapshot != null ? JsonSerializer.Serialize(subjectSnapshot) : "[]",
                    Outcome = PromotionStatus.PromotionPending,
                    CreatedAt = DateTime.UtcNow
                };
                
                context.LearnerAcademicRecords.Add(historyRecord);

                // Update Learner State
                learner.PromotionStatus = PromotionStatus.PromotionPending;
                learner.RegisterClassId = null;
                learner.CombinationId = null;
                
                // Clear subjects
                if (learner.LearnerSubjects != null)
                {
                    context.LearnerSubjects.RemoveRange(learner.LearnerSubjects);
                }
            }

            school.IsYearEndMode = true;
            await context.SaveChangesAsync();
            
            logger.LogInformation("School {SchoolId} entered Year End Mode for year {Year}.", schoolId, academicYear);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error entering Year End Mode for school {SchoolId}.", schoolId);
            return false;
        }
    }

    public async Task<bool> PromoteLearnerAsync(Guid learnerId, PromotionStatus outcome, string? comment = null)
    {
        try
        {
             await using var context = await dbContextFactory.CreateDbContextAsync();
             var learner = await context.Learners.FindAsync(learnerId);
             if (learner == null) return false;

             learner.PromotionStatus = outcome;
             
             // Update the most recent AcademicRecord
             var lastRecord = await context.LearnerAcademicRecords
                 .Where(r => r.LearnerId == learnerId)
                 .OrderByDescending(r => r.CreatedAt)
                 .FirstOrDefaultAsync();

             if (lastRecord != null && lastRecord.Outcome == PromotionStatus.PromotionPending)
             {
                 lastRecord.Outcome = outcome;
                 lastRecord.Comment = comment;
             }
             
             await context.SaveChangesAsync();
             return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error promoting learner {LearnerId}", learnerId);
            return false;
        }
    }

    public async Task<SchoolGrade?> GetNextGradeForLearnerAsync(Guid learnerId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            var learner = await context.Learners
                .Include(l => l.RegisterClass)
                    .ThenInclude(rc => rc!.SchoolGrade)
                        .ThenInclude(sg => sg!.SystemGrade)
                .Include(l => l.School)
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner == null || learner.RegisterClass?.SchoolGrade == null)
            {
                logger.LogWarning("Learner {LearnerId} not found or has no current grade.", learnerId);
                return null;
            }

            var currentSystemGrade = learner.RegisterClass.SchoolGrade.SystemGrade!;
            var nextSequenceNumber = currentSystemGrade.SequenceNumber + 1;

            // Find the next grade in the same school
            var nextGrade = await context.SchoolGrades
                .Include(sg => sg.SystemGrade)
                .Where(sg => sg.SchoolId == learner.SchoolId 
                    && sg.SystemGrade.SequenceNumber == nextSequenceNumber)
                .FirstOrDefaultAsync();

            return nextGrade;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting next grade for learner {LearnerId}", learnerId);
            return null;
        }
    }

    public async Task<bool> ProcessLearnerPromotionAsync(Guid learnerId, bool promote, Guid? targetGradeId = null, string? comment = null)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            var learner = await context.Learners
                .Include(l => l.LearnerSubjects)
                .Include(l => l.RegisterClass)
                    .ThenInclude(rc => rc!.SchoolGrade)
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner == null)
            {
                logger.LogWarning("Learner {LearnerId} not found.", learnerId);
                return false;
            }

            if (learner.PromotionStatus != PromotionStatus.PromotionPending)
            {
                logger.LogWarning("Learner {LearnerId} is not in PromotionPending status.", learnerId);
                return false;
            }

            if (promote)
            {
                // Promote the learner
                learner.PromotionStatus = PromotionStatus.Promoted;
                learner.Status = LearnerStatus.Active; // Restore to Active status
                
                // If a target grade is specified, we'll need to assign them to a class in that grade
                // For now, we clear their current assignments
                learner.RegisterClassId = null;
                learner.CombinationId = null;
            }
            else
            {
                // Retain the learner - they stay in the same grade
                learner.PromotionStatus = PromotionStatus.Retained;
                learner.Status = LearnerStatus.Active; // Restore to Active status
                // Keep their register class but clear subjects for reassignment
            }

            // Clear all subjects (they should already be cleared during year-end activation, but double-check)
            if (learner.LearnerSubjects != null && learner.LearnerSubjects.Any())
            {
                context.LearnerSubjects.RemoveRange(learner.LearnerSubjects);
            }

            // Update the academic record
            var lastRecord = await context.LearnerAcademicRecords
                .Where(r => r.LearnerId == learnerId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastRecord != null && lastRecord.Outcome == PromotionStatus.PromotionPending)
            {
                lastRecord.Outcome = promote ? PromotionStatus.Promoted : PromotionStatus.Retained;
                lastRecord.Comment = comment;
                lastRecord.ProcessedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            
            logger.LogInformation("Learner {LearnerId} promotion processed: {Outcome}", learnerId, promote ? "Promoted" : "Retained");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing promotion for learner {LearnerId}", learnerId);
            return false;
        }
    }

    public async Task<List<Learner>> GetLearnersAwaitingPromotionAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            return await context.Learners
                .Include(l => l.RegisterClass)
                    .ThenInclude(rc => rc!.SchoolGrade)
                        .ThenInclude(sg => sg!.SystemGrade)
                .Where(l => l.SchoolId == schoolId && 
                           l.Status == LearnerStatus.YearEndArchived &&
                           l.PromotionStatus == PromotionStatus.PromotionPending)
                .OrderBy(l => l.Surname)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting learners awaiting promotion for school {SchoolId}", schoolId);
            return [];
        }
    }

    public async Task<Learner?> GetLearnerByIdAsync(Guid learnerId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            return await context.Learners
                .Include(l => l.RegisterClass)
                    .ThenInclude(rc => rc!.SchoolGrade)
                        .ThenInclude(sg => sg!.SystemGrade)
                .Include(l => l.LearnerSubjects!)
                    .ThenInclude(ls => ls.Subject)
                .FirstOrDefaultAsync(l => l.Id == learnerId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting learner {LearnerId}", learnerId);
            return null;
        }
    }
}
