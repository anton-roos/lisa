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

    public async Task<List<Learner>> GetAdiRosterAsync(Guid academicDevelopmentClassId)
    {
        var adi = await academicDevelopmentClassService.GetByIdAsync(academicDevelopmentClassId)
                  ?? throw new KeyNotFoundException($"ADI event {academicDevelopmentClassId} not found.");

        // Roster is learners in grade who take the subject.
        var learners = await learnerService.GetByGradeAndSubjectAsync(adi.SchoolGradeId, adi.SubjectId);

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
}
