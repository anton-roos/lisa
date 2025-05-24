using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AttendanceRecordService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<AttendanceService> logger)
{
    public async Task CreateAsync(AttendanceRecord attendanceRecord)
    {
        attendanceRecord.CreatedAt = DateTime.UtcNow;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AttendanceRecords.Add(attendanceRecord);

        logger.LogInformation("Attendance Record {attendanceRecordId}", attendanceRecord.Id);

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<AttendanceRecord>> GetLeaveEarlyAttendancesAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDate,
        Guid? registerClassId = null,
        string? searchTerm = null,
        int skip = 0,
        int take = 50)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var query = dbContext.AttendanceRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ar => ar.Attendance)
            .Include(a => a.Learner!)
            .ThenInclude(l => l.RegisterClass)
            .Where(a => a.Attendance.SchoolId == schoolId &&
                        a.AttendanceType == AttendanceType.CheckIn &&
                        a.End != null &&
                        a.Start >= fromDate &&
                        a.Start <= toDate);

        if (registerClassId.HasValue)
        {
            query = query.Where(a => a.Learner!.RegisterClassId == registerClassId);
        }

        if (string.IsNullOrWhiteSpace(searchTerm))
            return await query
                .OrderByDescending(a => a.Start)
                .ThenByDescending(a => a.End)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(a => (a.Learner!.Name != null && a.Learner.Name.ToLower().Contains(searchTerm)) ||
                                     (a.Learner.Surname != null && a.Learner.Surname.ToLower().Contains(searchTerm)));
        }

        return await query
            .OrderByDescending(a => a.Start)
            .ThenByDescending(a => a.End)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> RecordSignOutAsync(Guid attendanceId, bool isEarlyLeave, string? notes = null)
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

    public async Task<bool> ClearSignOutAsync(Guid attendanceId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.Attendances.FindAsync(attendanceId);

        if (attendance == null)
        {
            logger.LogWarning("Attempted to clear sign-out for non-existent attendance record {AttendanceId}",
                attendanceId);
            return false;
        }

        attendance.End = null;
        attendance.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Cleared sign-out for attendance {AttendanceId}", attendanceId);

        return true;
    }
}