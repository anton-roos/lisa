using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AttendanceService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<AttendanceService> logger
)
{
    public async Task<Attendance?> GetTodaysAttendance(Guid? schoolId)
    {
        var today = DateTime.UtcNow.Date;
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances
            .Include(a => a.School)
            .Include(a => a.AttendanceRecords)
            .FirstOrDefaultAsync(a => a.Start.Date == today
                                      && a.SchoolId == schoolId
                                      && a.Type == AttendanceType.CheckIn);

        return attendance;
    }

    public async Task<Attendance> CreateAttendanceAsync(
        Guid schoolId,
        DateTime start,
        DateTime? end = null
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
            .ThenInclude(ar => ar.Learner)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<Attendance?> GetTodaysAttendanceAsync(Guid schoolId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .Include(s => s.School)
            .Include(s => s.AttendanceRecords)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId &&
                                      s.Start.Date == DateTime.UtcNow.Date &&
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

        session.End = endTime;
        session.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Updated end time for session {SessionId}", attendanceId);

        return session;
    }

    public async Task<List<Attendance>> GetRecentAttendancesAsync(Guid schoolId, int count = 10)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .Include(s => s.School)
            .Include(s => s.AttendanceRecords)
            .Where(s => s.SchoolId == schoolId)
            .OrderByDescending(s => s.Start)
            .Take(count)
            .ToListAsync();
    }
}