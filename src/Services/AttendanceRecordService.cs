using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AttendanceRecordService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<AttendanceService> logger
)
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

    public async Task<bool> SignOutAttendenceRecordAsync(Guid? attendanceRecordId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var attendance = await dbContext.AttendanceRecords.FindAsync(attendanceRecordId);

        if (attendance == null)
        {
            logger.LogWarning("Attempted to sign out non-existent attendance record {AttendanceId}", attendanceRecordId);
            return false;
        }

        attendance.End = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Recorded sign-out for attendance {AttendanceId}", attendanceRecordId);

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

    public async Task<bool> ToggleCellPhoneCollectedAsync(Guid attendanceRecordId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var record = await dbContext.AttendanceRecords.FindAsync(attendanceRecordId);

        if (record == null)
        {
            logger.LogWarning("Attempted to toggle cellphone collection for non-existent attendance record {AttendanceRecordId}", attendanceRecordId);
            return false;
        }

        record.CellPhoneCollected = !record.CellPhoneCollected;
        record.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Toggled cellphone collection for attendance record {AttendanceRecordId} to {CellPhoneCollected}",
            attendanceRecordId, record.CellPhoneCollected);

        return true;
    }

    public async Task<bool> ToggleCellPhoneReturnedAsync(Guid attendanceRecordId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var record = await dbContext.AttendanceRecords.FindAsync(attendanceRecordId);

        if (record == null)
        {
            logger.LogWarning("Attempted to toggle cellphone return for non-existent attendance record {AttendanceRecordId}", attendanceRecordId);
            return false;
        }

        record.CellPhoneReturned = !record.CellPhoneReturned;
        record.CellPhoneReturnedAt = record.CellPhoneReturned ? DateTime.UtcNow : null;
        record.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Toggled cellphone return for attendance record {AttendanceRecordId} to {CellPhoneReturned}",
            attendanceRecordId, record.CellPhoneReturned);

        return true;
    }

    public async Task<AttendanceRecord?> GetByIdAsync(Guid attendanceRecordId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.AttendanceRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attendanceRecordId);
    }

    public async Task<bool> UpdateAsync(AttendanceRecord attendanceRecord)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            // Make sure record exists
            var existingRecord = await dbContext.AttendanceRecords.FindAsync(attendanceRecord.Id);
            if (existingRecord == null)
            {
                logger.LogWarning("Attempted to update non-existent attendance record {AttendanceRecordId}",
                    attendanceRecord.Id);
                return false;
            }            // Update properties
            existingRecord.Start = attendanceRecord.Start;
            existingRecord.End = attendanceRecord.End;
            existingRecord.Notes = attendanceRecord.Notes;
            existingRecord.CellPhoneCollected = attendanceRecord.CellPhoneCollected;
            existingRecord.CellPhoneModel = attendanceRecord.CellPhoneModel;
            existingRecord.CellPhoneReturned = attendanceRecord.CellPhoneReturned;
            existingRecord.CellPhoneReturnedAt = attendanceRecord.CellPhoneReturnedAt;
            existingRecord.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Updated attendance record {AttendanceRecordId}", attendanceRecord.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating attendance record {AttendanceRecordId}", attendanceRecord.Id);
            return false;
        }
    }

    public async Task<bool> CollectCellPhoneWithModelAsync(Guid attendanceRecordId, string phoneModel)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var record = await dbContext.AttendanceRecords.FindAsync(attendanceRecordId);

        if (record == null)
        {
            logger.LogWarning("Attempted to collect cellphone for non-existent attendance record {AttendanceRecordId}", attendanceRecordId);
            return false;
        }

        record.CellPhoneCollected = true;
        record.CellPhoneModel = phoneModel;
        record.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Collected cellphone for attendance record {AttendanceRecordId} with model {CellPhoneModel}",
            attendanceRecordId, phoneModel);

        return true;
    }

    public async Task<List<AttendanceRecord>> GetTodaysLeaveEarlyAttendancesAsync(
    Guid schoolId,

    Guid? registerClassId = null,
    Guid? gradeId = null,
    string? searchTerm = null,
    int skip = 0,
    int take = 50)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();


        var today = DateTime.UtcNow.Date;
        //var today = DateTime.SpecifyKind(new DateTime(2025, 6, 17), DateTimeKind.Utc);
        var tomorrow = today.AddDays(1);

        var query = dbContext.AttendanceRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ar => ar.Attendance)
            .Include(ar => ar.Learner!)
            .ThenInclude(l => l.RegisterClass)
            .Where(a =>
                a.Attendance.SchoolId == schoolId &&
                a.AttendanceType == AttendanceType.CheckIn &&
                a.CreatedAt >= today &&
                a.CreatedAt < tomorrow);

        if (gradeId.HasValue)
        {
            query = query.Where(ar => ar.Learner!.RegisterClass!.SchoolGradeId == gradeId.Value);
        }

        if (registerClassId.HasValue)
        {
            query = query.Where(ar => ar.Learner!.RegisterClassId == registerClassId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(a =>
                (a.Learner!.Name != null && a.Learner.Name.ToLower().Contains(term)) ||
                (a.Learner.Surname != null && a.Learner.Surname.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}