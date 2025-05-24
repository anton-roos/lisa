using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public partial class AttendanceService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<AttendanceService> logger
)
{
    public async Task<bool> InitiatedAttendanceForToday(Guid? schoolId)
    {
        var today = DateTime.Now.Date;
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances
            .FirstOrDefaultAsync(a => a.Start.Date == today && a.SchoolId == schoolId);

        return attendance != null;
    }

    public async Task<Attendance> CreateAttendanceAsync(
        Guid schoolId,
        DateTimeOffset start,
        DateTimeOffset? end = null
    )
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var session = new Attendance
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            Start = start,
            End = end,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await dbContext.Attendances.AddAsync(session);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new attendance session {SessionId} for school {SchoolId}",
            session.Id, schoolId);

        return session;
    }

    public async Task<bool> RecordSignOutAsync(Guid attendanceId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            logger.LogWarning("Attempted to sign out non-existent attendance record {AttendanceId}", attendanceId);
            return false;
        }

        attendance.End = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Recorded sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<Attendance?> GetAsync(Guid sessionId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .Include(s => s.School)
            .Include(ar => ar.AttendanceRecords)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<Attendance?> GetActiveAttendanceAsync(Guid schoolId, DateTimeOffset date)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId &&
                                      s.Start == date &&
                                      s.End == null);
    }

    public async Task<bool> EndAttendanceAsync(Guid sessionId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var session = await dbContext.Attendances.FindAsync(sessionId);

        if (session == null)
        {
            logger.LogWarning("Attempted to end non-existent session {SessionId}", sessionId);
            return false;
        }

        session.End = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Ended attendance session {SessionId}", sessionId);
        return true;
    }

    public async Task<Attendance> UpdateAttendanceEndTimeAsync(Guid attendanceId, DateTime endTime)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var session = await dbContext.Attendances.FindAsync(attendanceId);

        if (session == null)
        {
            throw new KeyNotFoundException($"Attendance session with ID {attendanceId} not found");
        }

        endTime = endTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(endTime, DateTimeKind.Utc)
            : endTime.ToUniversalTime();

        session.End = endTime;
        session.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Updated end time for session {SessionId}", attendanceId);

        return session;
    }

    public async Task<List<Attendance>> GetCompletedSessionForSchoolAsync(Guid schoolId, DateTimeOffset date)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .Where(s => s.SchoolId == schoolId &&
                        s.Start == date.Date &&
                        s.End != null)
            .ToListAsync();
    }
}