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
    /// <summary>
    /// DEPRECATED: Use SchoolService.ActivateYearEndModeAsync instead.
    /// This method is kept for reference but should not be called.
    /// </summary>
    [Obsolete("Use SchoolService.ActivateYearEndModeAsync instead which properly handles academic year transitions.")]
    public async Task<bool> EnterYearEndModeAsync(Guid schoolId, int academicYear)
    {
        // This method is deprecated - use SchoolService.ActivateYearEndModeAsync
        logger.LogWarning("EnterYearEndModeAsync is deprecated. Use SchoolService.ActivateYearEndModeAsync instead.");
        return false;
    }

    public async Task<bool> PromoteLearnerAsync(Guid learnerId, PromotionStatus outcome, string? comment = null)
    {
        try
        {
             await using var context = await dbContextFactory.CreateDbContextAsync();
             var learner = await context.Learners.FindAsync(learnerId);
             if (learner == null) return false;

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
                .Include(l => l.PreviousSchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(l => l.School)
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner == null)
            {
                logger.LogWarning("Learner {LearnerId} not found.", learnerId);
                return null;
            }

            // Use PreviousSchoolGrade if available (year-end mode), otherwise use current RegisterClass grade
            SchoolGrade? currentGrade = learner.PreviousSchoolGrade ?? learner.RegisterClass?.SchoolGrade;
            
            if (currentGrade?.SystemGrade == null)
            {
                logger.LogWarning("Learner {LearnerId} has no current or previous grade.", learnerId);
                return null;
            }

            var currentSystemGrade = currentGrade.SystemGrade;
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
                        .ThenInclude(sg => sg!.SystemGrade)
                .Include(l => l.School)
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner == null)
            {
                logger.LogWarning("Learner {LearnerId} not found.", learnerId);
                return false;
            }

            if (learner.Status != LearnerStatus.PendingPromotion)
            {
                logger.LogWarning("Learner {LearnerId} is not in PendingPromotion status.", learnerId);
                return false;
            }

            if (promote)
            {
                // Check if learner is in the highest grade of the school
                var currentGradeSequence = learner.RegisterClass?.SchoolGrade?.SystemGrade?.SequenceNumber;
                
                if (currentGradeSequence.HasValue)
                {
                    // Get the highest grade in the school
                    var highestGrade = await context.SchoolGrades
                        .Include(sg => sg.SystemGrade)
                        .Where(sg => sg.SchoolId == learner.SchoolId)
                        .OrderByDescending(sg => sg.SystemGrade.SequenceNumber)
                        .FirstOrDefaultAsync();

                    if (highestGrade != null && currentGradeSequence.Value == highestGrade.SystemGrade.SequenceNumber)
                    {
                        // Learner is graduating from the highest grade
                        learner.Status = LearnerStatus.Graduated;
                        logger.LogInformation("Learner {LearnerId} is graduating from the highest grade.", learnerId);
                    }
                    else
                    {
                        // Normal promotion - mark as Promoted (temporary status)
                        learner.Status = LearnerStatus.Promoted;
                    }
                }
                else
                {
                    // Fallback if no grade info available
                    learner.Status = LearnerStatus.Promoted;
                }
                
                // Clear current assignments - will be reassigned to new grade
                learner.RegisterClassId = null;
                learner.CombinationId = null;
            }
            else
            {
                // Retain the learner - they stay in the same grade
                learner.Status = LearnerStatus.Retained;
                // Keep their register class reference for now, but clear combination
                learner.CombinationId = null;
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
            
            var outcome = learner.Status == LearnerStatus.Graduated ? "Graduated" : (promote ? "Promoted" : "Retained");
            logger.LogInformation("Learner {LearnerId} promotion processed: {Outcome}", learnerId, outcome);
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
                           l.Status == LearnerStatus.PendingPromotion)
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
                .Include(l => l.PreviousSchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(l => l.PreviousRegisterClass)
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
