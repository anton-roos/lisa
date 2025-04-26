using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AttendanceService
{
    private readonly LisaDbContext _dbContext;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(LisaDbContext dbContext, ILogger<AttendanceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Attendance> RecordAttendanceAsync(Guid learnerId, Guid schoolId, Guid registerClassId,
        bool isPresent, string? notes = null, Guid? recordedByUserId = null, Guid? sessionId = null)
    {
        // Check if attendance already exists for today
        var today = DateTime.UtcNow.Date;
        var existingAttendance = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.LearnerId == learnerId &&
                                    a.RegisterClassId == registerClassId &&
                                    a.Date.Date == today);

        if (existingAttendance != null)
        {
            // Update existing attendance
            existingAttendance.IsPresent = isPresent;
            existingAttendance.Notes = notes;
            existingAttendance.UpdatedAt = DateTime.UtcNow;
            existingAttendance.UpdatedBy = recordedByUserId;

            // If marking present and no sign-in time, update it
            if (isPresent && existingAttendance.SignInTime == null)
            {
                existingAttendance.SignInTime = DateTime.UtcNow;
            }

            // Update session ID if provided
            if (sessionId.HasValue && existingAttendance.AttendanceSessionId != sessionId)
            {
                existingAttendance.AttendanceSessionId = sessionId;
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated attendance for learner {LearnerId}", learnerId);
            return existingAttendance;
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
            Notes = notes,
            RecordedByUserId = recordedByUserId,
            AttendanceSessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = recordedByUserId,
            UpdatedBy = recordedByUserId
        };

        await _dbContext.Attendances.AddAsync(attendance);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new attendance record for learner {LearnerId}", learnerId);
        return attendance;
    }

    public async Task<Attendance?> GetAttendanceAsync(Guid learnerId, Guid registerClassId, DateTime date)
    {
        return await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.LearnerId == learnerId &&
                                     a.RegisterClassId == registerClassId &&
                                     a.Date.Date == date.Date);
    }

    public async Task<Attendance> UpdateAttendanceAsync(Guid attendanceId, bool isPresent,
        bool isEarlyLeave = false, string? notes = null)
    {
        var attendance = await _dbContext.Attendances.FindAsync(attendanceId)
            ?? throw new KeyNotFoundException($"Attendance record with ID {attendanceId} not found");

        attendance.IsPresent = isPresent;
        attendance.IsEarlyLeave = isEarlyLeave;

        if (notes != null)
        {
            attendance.Notes = notes;
        }

        attendance.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated attendance {AttendanceId}", attendanceId);

        return attendance;
    }

    public async Task<bool> RecordSignOutAsync(Guid attendanceId)
    {
        var attendance = await _dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            _logger.LogWarning("Attempted to sign out non-existent attendance record {AttendanceId}", attendanceId);
            return false;
        }

        attendance.SignOutTime = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Recorded sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<AttendanceSession> CreateAttendanceSessionAsync(Guid schoolId, DateTime startTime, Guid? createdByUserId)
    {
        // Check if there's already a session for this school and date
        var existingSession = await GetActiveSessionForSchoolAsync(schoolId, startTime.Date);
        if (existingSession != null)
        {
            return existingSession;
        }

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

        await _dbContext.AttendanceSessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new attendance session {SessionId} for school {SchoolId}",
            session.Id, schoolId);

        return session;
    }

    public async Task<AttendanceSession?> GetAttendanceSessionAsync(Guid sessionId)
    {
        return await _dbContext.AttendanceSessions
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<AttendanceSession?> GetActiveSessionForSchoolAsync(Guid schoolId, DateTime date)
    {
        // Convert input date to UTC before querying
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        return await _dbContext.AttendanceSessions
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId &&
                                      s.Date.Date == utcDate.Date &&
                                      s.EndTime == null);
    }

    public async Task<List<Attendance>> GetAttendancesForSessionAsync(Guid sessionId)
    {
        return await _dbContext.Attendances
            .Where(a => a.AttendanceSessionId == sessionId)
            .ToListAsync();
    }

    public async Task<bool> EndSessionAsync(Guid sessionId)
    {
        var session = await _dbContext.AttendanceSessions.FindAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Attempted to end non-existent session {SessionId}", sessionId);
            return false;
        }

        session.EndTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Ended attendance session {SessionId}", sessionId);
        return true;
    }

    public async Task<bool> RecordSignOutAsync(Guid attendanceId, bool isEarlyLeave, string? notes = null)
    {
        var attendance = await _dbContext.Attendances.FindAsync(attendanceId);

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

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Recorded sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<bool> ClearSignOutAsync(Guid attendanceId)
    {
        var attendance = await _dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            _logger.LogWarning("Attempted to clear sign-out for non-existent attendance record {AttendanceId}", attendanceId);
            return false;
        }

        attendance.SignOutTime = null;
        attendance.IsEarlyLeave = false;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Cleared sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }

    public async Task<AttendanceSession> UpdateSessionEndTimeAsync(Guid sessionId, DateTime endTime)
    {
        var session = await _dbContext.AttendanceSessions.FindAsync(sessionId);

        if (session == null)
        {
            throw new KeyNotFoundException($"Attendance session with ID {sessionId} not found");
        }

        session.EndTime = endTime;
        session.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated end time for session {SessionId}", sessionId);

        return session;
    }

    public async Task<AttendanceSession?> GetCompletedSessionForSchoolAsync(Guid schoolId, DateTime date)
    {
        // Convert input date to UTC before querying
        var utcDate = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        return await _dbContext.AttendanceSessions
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId &&
                                    s.Date.Date == utcDate.Date &&
                                    s.EndTime != null);
    }
}