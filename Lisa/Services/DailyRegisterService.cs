using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class DailyRegisterService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<DailyRegisterService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<DailyRegisterService> _logger = logger;

    /// <summary>
    /// Gets register classes for a school.
    /// </summary>
    public async Task<List<RegisterClass>> GetRegisterClassesBySchoolIdAsync(Guid schoolId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.SchoolGrade!)
                    .ThenInclude(sg => sg.SystemGrade)
                .Where(rc => rc.SchoolGrade != null && rc.SchoolGrade.SchoolId == schoolId)
                .OrderBy(rc => rc.SchoolGrade!.SystemGrade.SequenceNumber)
                .ThenBy(rc => rc.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting register classes for school {SchoolId}", schoolId);
            return [];
        }
    }

    /// <summary>
    /// Gets the learners in a register class.
    /// </summary>
    public async Task<List<Learner>> GetLearnersByRegisterClassAsync(Guid registerClassId)
    {
        try 
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Learners
                .AsNoTracking()
                .AsSplitQuery()
                .Where(l => l.RegisterClassId == registerClassId && l.Active)
                .OrderBy(l => l.Surname)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learners for register class {RegisterClassId}", registerClassId);
            return [];
        }
    }

    /// <summary>
    /// Gets attendance records for learners on a specific date.
    /// </summary>
    public async Task<Dictionary<Guid, Attendance>> GetAttendanceRecordsAsync(
        Guid registerClassId, 
        List<Guid> learnerIds, 
        DateTime date)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            // Get UTC date for comparison
            var utcDate = date.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                : date.ToUniversalTime();
            
            // Get attendance records for the specified date
            var attendanceRecords = await context.Attendances
                .AsNoTracking()
                .Where(a => 
                    a.RegisterClassId == registerClassId &&
                    learnerIds.Contains(a.LearnerId) &&
                    a.Date.Date == utcDate.Date)
                .ToListAsync();
            
            return attendanceRecords.ToDictionary(a => a.LearnerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance records for register class {RegisterClassId} on {Date}", 
                registerClassId, date);
            return [];
        }
    }

    /// <summary>
    /// Records attendance for a learner.
    /// </summary>
    public async Task<Attendance> RecordAttendanceAsync(
        Guid learnerId, 
        Guid schoolId, 
        Guid registerClassId, 
        bool isPresent, 
        Guid? recordedByUserId = null)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var today = DateTime.UtcNow.Date;

            // Check if attendance record already exists using ExecuteUpdateAsync for better performance
            var updatedCount = await context.Attendances
                .Where(a => 
                    a.LearnerId == learnerId &&
                    a.RegisterClassId == registerClassId &&
                    a.Date.Date == today)
                .ExecuteUpdateAsync(s => 
                    s.SetProperty(a => a.IsPresent, isPresent)
                     .SetProperty(a => a.UpdatedAt, DateTime.UtcNow)
                     .SetProperty(a => a.UpdatedBy, recordedByUserId));
            
            if (updatedCount > 0)
            {
                _logger.LogInformation("Updated attendance for learner {LearnerId} to {IsPresent}", 
                    learnerId, isPresent);
                
                // Fetch the updated record to return
                var existingAttendance = await context.Attendances
                    .FirstOrDefaultAsync(a => 
                        a.LearnerId == learnerId &&
                        a.RegisterClassId == registerClassId &&
                        a.Date.Date == today);
                
                if (existingAttendance != null)
                {
                    return existingAttendance;
                }
                
                // This should not happen, but just in case
                _logger.LogWarning("Failed to find attendance record after update for learner {LearnerId}", learnerId);
            }

            // Create new attendance record
            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                LearnerId = learnerId,
                SchoolId = schoolId,
                RegisterClassId = registerClassId,
                Date = today,
                IsPresent = isPresent,
                SignInTime = isPresent ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = recordedByUserId,
                UpdatedBy = recordedByUserId
            };

            await context.Attendances.AddAsync(attendance);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Created new attendance record for learner {LearnerId} with status {IsPresent}", 
                learnerId, isPresent);
                
            return attendance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording attendance for learner {LearnerId}", learnerId);
            throw;
        }
    }
}
