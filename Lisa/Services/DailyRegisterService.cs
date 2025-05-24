using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class DailyRegisterService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<DailyRegisterService> logger
)
{
    public async Task<AttendanceRecord> RecordAttendanceAsync(
        Guid learnerId,
        Guid registerClassId,
        bool isPresent,
        Guid? recordedByUserId = null)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var today = DateTime.UtcNow.Date;

            var updatedCount = await context.AttendanceRecords
                .Where(a =>
                    a.LearnerId == learnerId &&
                    a.Learner!.RegisterClassId == registerClassId &&
                    a.Start == today)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(a => a.UpdatedAt, DateTime.UtcNow)
                     .SetProperty(a => a.UpdatedBy, recordedByUserId));

            if (updatedCount > 0)
            {
                logger.LogInformation("Updated attendance for learner {LearnerId} to {IsPresent}",
                    learnerId, isPresent);

                var existingAttendance = await context.AttendanceRecords
                    .FirstOrDefaultAsync(a =>
                        a.LearnerId == learnerId &&
                        a.Learner!.RegisterClassId == registerClassId &&
                        a.Start == today);

                if (existingAttendance != null)
                {
                    return existingAttendance;
                }

                logger.LogWarning("Failed to find attendance record after update for learner {LearnerId}", learnerId);
            }

            var attendance = new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                LearnerId = learnerId,
                Start = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = recordedByUserId,
                UpdatedBy = recordedByUserId
            };

            await context.AttendanceRecords.AddAsync(attendance);
            await context.SaveChangesAsync();

            logger.LogInformation("Created new attendance record for learner {LearnerId} with status {IsPresent}",
                learnerId, isPresent);

            return attendance;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error recording attendance for learner {LearnerId}", learnerId);
            throw;
        }
    }
}