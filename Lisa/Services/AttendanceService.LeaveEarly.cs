using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public partial class AttendanceService
{
    public async Task<List<Attendance>> GetLeaveEarlyAttendancesAsync(
        Guid schoolId, 
        DateTime fromDate, 
        DateTime toDate, 
        Guid? registerClassId = null,
        string? searchTerm = null,
        int skip = 0,
        int take = 50)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var query = dbContext.Attendances
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Learner!)
                .ThenInclude(l => l.RegisterClass)
            .Include(a => a.RecordedByUser)
            .Where(a => a.SchoolId == schoolId && 
                      a.SignOutTime != null &&
                      a.Date >= fromDate && 
                      a.Date <= toDate);
        
        if (registerClassId.HasValue)
        {
            query = query.Where(a => a.Learner!.RegisterClassId == registerClassId);
        }
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(a => (a.Learner!.Name != null && a.Learner.Name.ToLower().Contains(searchTerm)) || 
                                   (a.Learner.Surname != null && a.Learner.Surname.ToLower().Contains(searchTerm)));
        }
        
        return await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.SignOutTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
    
    public async Task<int> GetLeaveEarlyAttendancesCountAsync(
        Guid schoolId, 
        DateTime fromDate, 
        DateTime toDate, 
        Guid? registerClassId = null,
        string? searchTerm = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var query = dbContext.Attendances
            .AsNoTracking()
            .Where(a => a.SchoolId == schoolId && 
                      a.SignOutTime != null &&
                      a.Date >= fromDate && 
                      a.Date <= toDate);
        
        if (registerClassId.HasValue)
        {
            query = query.Where(a => a.Learner!.RegisterClassId == registerClassId);
        }
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(a => (a.Learner!.Name != null && a.Learner.Name.ToLower().Contains(searchTerm)) || 
                                   (a.Learner.Surname != null && a.Learner.Surname.ToLower().Contains(searchTerm)));
        }
        
        return await query.CountAsync();
    }
}
