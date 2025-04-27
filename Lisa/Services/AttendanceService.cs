using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public partial class AttendanceService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<AttendanceService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<AttendanceService> _logger = logger;

    public async Task<Attendance> RecordAttendanceAsync(Guid learnerId, Guid schoolId, Guid registerClassId,
        bool isPresent, string? notes = null, Guid? recordedByUserId = null, Guid? sessionId = null)
    {
        var today = DateTime.UtcNow.Date;
        
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingAttendance = await dbContext.Attendances
            .FirstOrDefaultAsync(a => a.LearnerId == learnerId &&
                                    a.RegisterClassId == registerClassId &&
                                    a.Date.Date == today);

        if (existingAttendance != null)
        {
            existingAttendance.IsPresent = isPresent;
            existingAttendance.Notes = notes;
            existingAttendance.UpdatedAt = DateTime.UtcNow;
            existingAttendance.UpdatedBy = recordedByUserId;

            if (isPresent && existingAttendance.SignInTime == null)
            {
                existingAttendance.SignInTime = DateTime.UtcNow;
            }

            if (sessionId.HasValue && existingAttendance.AttendanceSessionId != sessionId)
            {
                existingAttendance.AttendanceSessionId = sessionId;
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated attendance for learner {LearnerId}", learnerId);
            return existingAttendance;
        }

        var attendance = new Attendance
        {
            Id = Guid.NewGuid(),
            LearnerId = learnerId,
            SchoolId = schoolId,
            RegisterClassId = registerClassId,
            Date = today,
            IsPresent = isPresent,
            SignInTime = isPresent ? DateTime.UtcNow : null,
            Notes = notes,
            RecordedByUserId = recordedByUserId,
            AttendanceSessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = recordedByUserId,
            UpdatedBy = recordedByUserId
        };

        await dbContext.Attendances.AddAsync(attendance);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new attendance record for learner {LearnerId}", learnerId);
        return attendance;
    }

    public async Task<Attendance?> GetAttendanceAsync(Guid learnerId, Guid registerClassId, DateTime date)
    {
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .FirstOrDefaultAsync(a => a.LearnerId == learnerId &&
                                     a.RegisterClassId == registerClassId &&
                                     a.Date.Date == utcDate.Date);
    }

    public async Task<Attendance> UpdateAttendanceAsync(Guid attendanceId, bool isPresent,
        bool isEarlyLeave = false, string? notes = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances.FindAsync(attendanceId)
            ?? throw new KeyNotFoundException($"Attendance record with ID {attendanceId} not found");

        attendance.IsPresent = isPresent;
        attendance.IsEarlyLeave = isEarlyLeave;

        if (notes != null)
        {
            attendance.Notes = notes;
        }

        // Ensure we're using UTC time for updates
        attendance.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated attendance {AttendanceId}", attendanceId);

        return attendance;
    }

    public async Task<bool> RecordSignOutAsync(Guid attendanceId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            _logger.LogWarning("Attempted to sign out non-existent attendance record {AttendanceId}", attendanceId);
            return false;
        }

        attendance.SignOutTime = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Recorded sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<AttendanceSession> CreateAttendanceSessionAsync(Guid schoolId, DateTime startTime, Guid? createdByUserId)
    {
        // Ensure startTime is in UTC
        startTime = startTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(startTime, DateTimeKind.Utc)
            : startTime.ToUniversalTime();

        // Check if there's already a session for this school and date
        var existingSession = await GetActiveSessionForSchoolAsync(schoolId, startTime.Date);
        if (existingSession != null)
        {
            return existingSession;
        }

        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var session = new AttendanceSession
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            Date = startTime.Date,
            StartTime = startTime,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdByUserId
        };

        await dbContext.AttendanceSessions.AddAsync(session);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new attendance session {SessionId} for school {SchoolId}",
            session.Id, schoolId);

        return session;
    }

    public async Task<AttendanceSession?> GetAttendanceSessionAsync(Guid sessionId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.AttendanceSessions
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<AttendanceSession?> GetActiveSessionForSchoolAsync(Guid schoolId, DateTime date)
    {
        // Convert input date to UTC before querying
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.AttendanceSessions
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId &&
                                      s.Date.Date == utcDate.Date &&
                                      s.EndTime == null);
    }

    public async Task<List<Attendance>> GetAttendancesForSessionAsync(Guid sessionId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Attendances
            .Where(a => a.AttendanceSessionId == sessionId)
            .ToListAsync();
    }

    public async Task<bool> EndSessionAsync(Guid sessionId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var session = await dbContext.AttendanceSessions.FindAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Attempted to end non-existent session {SessionId}", sessionId);
            return false;
        }

        // Ensure we're using UTC time for session end
        session.EndTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Ended attendance session {SessionId}", sessionId);
        return true;
    }

    public async Task<bool> RecordSignOutAsync(Guid attendanceId, bool isEarlyLeave, string? notes = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            _logger.LogWarning("Attempted to sign out non-existent attendance record {AttendanceId}", attendanceId);
            return false;
        }

        attendance.SignOutTime = DateTime.UtcNow;
        attendance.IsEarlyLeave = isEarlyLeave;
        attendance.UpdatedAt = DateTime.UtcNow;

        if (notes != null)
        {
            attendance.Notes = notes;
        }

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Recorded sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<bool> ClearSignOutAsync(Guid attendanceId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            _logger.LogWarning("Attempted to clear sign-out for non-existent attendance record {AttendanceId}", attendanceId);
            return false;
        }

        attendance.SignOutTime = null;
        attendance.IsEarlyLeave = false;
        attendance.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Cleared sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<AttendanceSession> UpdateSessionEndTimeAsync(Guid sessionId, DateTime endTime)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var session = await dbContext.AttendanceSessions.FindAsync(sessionId);

        if (session == null)
        {
            throw new KeyNotFoundException($"Attendance session with ID {sessionId} not found");
        }

        endTime = endTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(endTime, DateTimeKind.Utc)
            : endTime.ToUniversalTime();

        session.EndTime = endTime;
        session.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated end time for session {SessionId}", sessionId);

        return session;
    }

    public async Task<AttendanceSession?> GetCompletedSessionForSchoolAsync(Guid schoolId, DateTime date)
    {
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.AttendanceSessions
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId &&
                                    s.Date.Date == utcDate.Date &&
                                    s.EndTime != null);
    }
}