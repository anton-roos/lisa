using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AdiAttendanceService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    AcademicDevelopmentClassService academicDevelopmentClassService,
    AttendanceService attendanceService,
    AttendanceRecordService attendanceRecordService,
    LearnerService learnerService,
    ILogger<AdiAttendanceService> logger
)
{
    public async Task<Attendance> GetOrCreateAdiAttendanceAsync(Guid academicDevelopmentClassId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var existing = await context.Attendances
            .Include(a => a.AttendanceRecords)
            .FirstOrDefaultAsync(a => a.AcademicDevelopmentClassId == academicDevelopmentClassId
                                      && a.Type == AttendanceType.Adi);

        if (existing != null)
        {
            return existing;
        }

        var adi = await academicDevelopmentClassService.GetByIdAsync(academicDevelopmentClassId)
                  ?? throw new KeyNotFoundException($"ADI event {academicDevelopmentClassId} not found.");

        // AcademicDevelopmentClassService stores DateTime as UTC; ensure we use UTC for attendance start.
        var startUtc = adi.DateTime.Kind == DateTimeKind.Utc
            ? adi.DateTime
            : adi.DateTime.Kind == DateTimeKind.Local
                ? adi.DateTime.ToUniversalTime()
                : DateTime.SpecifyKind(adi.DateTime, DateTimeKind.Utc);

        // Use the shared AttendanceService so AcademicYearId is set consistently.
        var created = await attendanceService.CreateAttendanceAsync(
            schoolId: adi.SchoolId,
            start: startUtc,
            end: null,
            type: AttendanceType.Adi,
            academicDevelopmentClassId: adi.Id);

        // Re-load with records (empty initially) for consistency.
        await using var context2 = await dbContextFactory.CreateDbContextAsync();
        return await context2.Attendances
                   .Include(a => a.AttendanceRecords)
                   .FirstAsync(a => a.Id == created.Id);
    }

    /// <summary>
    /// Get the roster of learners for an ADI class from the AdiLearners table.
    /// This returns learners explicitly assigned to the ADI, with their IsAdditional flag.
    /// Additional learners are shown at the top, ordered by when they were added (most recent first).
    /// </summary>
    public async Task<List<(Learner Learner, bool IsAdditional)>> GetAdiRosterWithAdditionalFlagAsync(Guid academicDevelopmentClassId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var adiLearners = await context.AdiLearners
            .Where(al => al.AcademicDevelopmentClassId == academicDevelopmentClassId)
            .Include(al => al.Learner!)
            .ThenInclude(l => l.RegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .AsNoTracking()
            .ToListAsync();

        // Order: Additional learners first (most recently added at top), then regular learners
        return adiLearners
            .Where(al => al.Learner != null)
            .Select(al => (al.Learner!, al.IsAdditional, al.CreatedAt))
            .OrderByDescending(x => x.IsAdditional) // Additional learners first
            .ThenByDescending(x => x.IsAdditional ? x.CreatedAt : DateTime.MinValue) // Most recent additional first
            .ThenBy(x => x.Item1.RegisterClass?.DisplayName)
            .ThenBy(x => x.Item1.Surname)
            .ThenBy(x => x.Item1.Name)
            .Select(x => (x.Item1, x.IsAdditional))
            .ToList();
    }

    /// <summary>
    /// Legacy method - returns just learners based on grade/subject combination.
    /// Used for backward compatibility. Consider using GetAdiRosterWithAdditionalFlagAsync instead.
    /// </summary>
    public async Task<List<Learner>> GetAdiRosterAsync(Guid academicDevelopmentClassId)
    {
        // First try to get from AdiLearners
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var adiLearnerCount = await context.AdiLearners
            .CountAsync(al => al.AcademicDevelopmentClassId == academicDevelopmentClassId);

        if (adiLearnerCount > 0)
        {
            var roster = await GetAdiRosterWithAdditionalFlagAsync(academicDevelopmentClassId);
            return roster.Select(r => r.Learner).ToList();
        }

        // Fallback to grade/subject based roster for older ADI classes without explicit learners
        var adi = await academicDevelopmentClassService.GetByIdAsync(academicDevelopmentClassId)
                  ?? throw new KeyNotFoundException($"ADI event {academicDevelopmentClassId} not found.");

        // If it's a Break ADI (no grade) or has no subject, return empty list - Break ADIs must use AdiLearners
        if (adi.SchoolGradeId == null || adi.SubjectId == null)
        {
            return [];
        }

        var learners = await learnerService.GetByGradeAndSubjectAsync(adi.SchoolGradeId.Value, adi.SubjectId.Value);

        return learners
            .Where(l => l.SchoolId == adi.SchoolId)
            .OrderBy(l => l.RegisterClass!.DisplayName)
            .ThenBy(l => l.Surname)
            .ThenBy(l => l.Name)
            .ToList();
    }

    public async Task<Dictionary<Guid, AttendanceRecord>> GetExistingAdiAttendanceRecordsAsync(Guid attendanceId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var records = await context.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.AttendanceId == attendanceId && r.AttendanceType == AttendanceType.Adi)
            .ToListAsync();

        return records
            .GroupBy(r => r.LearnerId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.UpdatedAt).First());
    }

    /// <summary>
    /// Record a learner's attendance arrival during active attendance.
    /// Marks with the current timestamp.
    /// </summary>
    public async Task MarkLearnerArrivedAsync(
        Guid attendanceId,
        Guid learnerId,
        Guid? userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        // Get the ADI to check attendance status
        var attendance = await context.Attendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId);
        
        if (attendance?.AcademicDevelopmentClassId == null)
        {
            throw new InvalidOperationException("Attendance not associated with an ADI class.");
        }

        var adi = await context.AcademicDevelopmentClasses
            .FirstOrDefaultAsync(a => a.Id == attendance.AcademicDevelopmentClassId);

        if (adi == null)
        {
            throw new KeyNotFoundException("ADI class not found.");
        }

        var arrivalTime = DateTime.UtcNow;
        var isLate = adi.AttendanceStoppedAt != null && arrivalTime > adi.AttendanceStoppedAt;
        var status = isLate ? "Late" : "Present";

        var existing = await context.AttendanceRecords
            .FirstOrDefaultAsync(r => r.AttendanceId == attendanceId
                                      && r.LearnerId == learnerId
                                      && r.AttendanceType == AttendanceType.Adi);

        var record = new AttendanceRecord
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            AttendanceId = attendanceId,
            LearnerId = learnerId,
            AttendanceType = AttendanceType.Adi,
            Start = arrivalTime,
            Notes = status,
            CreatedAt = existing?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = existing?.CreatedBy ?? userId,
            UpdatedBy = userId
        };

        if (existing == null)
        {
            await attendanceRecordService.CreateAsync(record);
            logger.LogInformation("Marked learner {LearnerId} as {Status} at {Time} for ADI attendance {AttendanceId}",
                learnerId, status, arrivalTime, attendanceId);
        }
        else
        {
            await attendanceRecordService.UpdateAsync(record);
            logger.LogInformation("Updated learner {LearnerId} to {Status} at {Time} for ADI attendance {AttendanceId}",
                learnerId, status, arrivalTime, attendanceId);
        }
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    public async Task SetLearnerAdiAttendanceAsync(
        Guid attendanceId,
        Guid learnerId,
        bool isPresent,
        Guid? userId)
    {
        // Find existing record if present.
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var existing = await context.AttendanceRecords
            .FirstOrDefaultAsync(r => r.AttendanceId == attendanceId
                                      && r.LearnerId == learnerId
                                      && r.AttendanceType == AttendanceType.Adi);

        var record = new AttendanceRecord
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            AttendanceId = attendanceId,
            LearnerId = learnerId,
            AttendanceType = AttendanceType.Adi,
            Start = DateTime.UtcNow,
            Notes = isPresent ? "Present" : "Absent",
            CreatedAt = existing?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = existing?.CreatedBy ?? userId,
            UpdatedBy = userId
        };

        if (existing == null)
        {
            await attendanceRecordService.CreateAsync(record);
            logger.LogInformation("Created ADI attendance record for learner {LearnerId} in attendance {AttendanceId} as {Status}",
                learnerId, attendanceId, record.Notes);
        }
        else
        {
            await attendanceRecordService.UpdateAsync(record);
            logger.LogInformation("Updated ADI attendance record for learner {LearnerId} in attendance {AttendanceId} as {Status}",
                learnerId, attendanceId, record.Notes);
        }
    }

    /// <summary>
    /// Add an additional learner to an ADI class during attendance.
    /// </summary>
    public async Task<AdiLearner> AddAdditionalLearnerAsync(
        Guid academicDevelopmentClassId, 
        Guid learnerId, 
        Guid? userId,
        string? breakReason = null)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        // Check if already exists
        var existing = await context.AdiLearners
            .FirstOrDefaultAsync(al => al.AcademicDevelopmentClassId == academicDevelopmentClassId
                                       && al.LearnerId == learnerId);

        if (existing != null)
        {
            return existing;
        }

        var adiLearner = new AdiLearner
        {
            Id = Guid.NewGuid(),
            AcademicDevelopmentClassId = academicDevelopmentClassId,
            LearnerId = learnerId,
            IsAdditional = true,
            BreakReason = breakReason,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        context.AdiLearners.Add(adiLearner);
        await context.SaveChangesAsync();

        logger.LogInformation("Added additional learner {LearnerId} to ADI {AdiId} with reason: {Reason}", 
            learnerId, academicDevelopmentClassId, breakReason ?? "(none)");

        return adiLearner;
    }

    /// <summary>
    /// Add learners to an ADI class (non-additional, assigned during creation).
    /// </summary>
    public async Task AddLearnersToAdiAsync(Guid academicDevelopmentClassId, IEnumerable<Guid> learnerIds, Guid? userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var existingIds = await context.AdiLearners
            .Where(al => al.AcademicDevelopmentClassId == academicDevelopmentClassId)
            .Select(al => al.LearnerId)
            .ToListAsync();

        var newLearnerIds = learnerIds.Except(existingIds).ToList();

        var adiLearners = newLearnerIds.Select(lid => new AdiLearner
        {
            Id = Guid.NewGuid(),
            AcademicDevelopmentClassId = academicDevelopmentClassId,
            LearnerId = lid,
            IsAdditional = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        }).ToList();

        if (adiLearners.Count != 0)
        {
            context.AdiLearners.AddRange(adiLearners);
            await context.SaveChangesAsync();
            logger.LogInformation("Added {Count} learners to ADI {AdiId}", adiLearners.Count, academicDevelopmentClassId);
        }
    }

    /// <summary>
    /// Remove a learner from an ADI class.
    /// </summary>
    public async Task RemoveLearnerFromAdiAsync(Guid academicDevelopmentClassId, Guid learnerId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var adiLearner = await context.AdiLearners
            .FirstOrDefaultAsync(al => al.AcademicDevelopmentClassId == academicDevelopmentClassId
                                       && al.LearnerId == learnerId);

        if (adiLearner != null)
        {
            context.AdiLearners.Remove(adiLearner);
            await context.SaveChangesAsync();
            logger.LogInformation("Removed learner {LearnerId} from ADI {AdiId}", learnerId, academicDevelopmentClassId);
        }
    }
}
