using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AcademicDevelopmentClassService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    SchoolService schoolService,
    ILogger<AcademicDevelopmentClassService> logger
)
{
    /// <summary>
    /// Get all academic development classes filtered by current academic year.
    /// </summary>
    public async Task<List<AcademicDevelopmentClass>> GetAllAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.AcademicDevelopmentClasses
                .Where(adc => adc.AcademicYear != null && adc.AcademicYear.IsCurrent)
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .Include(adc => adc.AdiSubjects)
                .ThenInclude(asub => asub.Subject)
                .Include(adc => adc.AdiTeachers)
                .ThenInclude(at => at.Teacher)
                .AsNoTracking()
                .OrderBy(adc => adc.DateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all academic development classes.");
            return [];
        }
    }

    public async Task<List<AcademicDevelopmentClass>> GetBySchoolIdAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            // Filter by current academic year
            return await context.AcademicDevelopmentClasses
                .Where(adc => adc.SchoolId == schoolId && 
                              adc.AcademicYear != null && adc.AcademicYear.IsCurrent)
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .Include(adc => adc.AdiSubjects)
                .ThenInclude(asub => asub.Subject)
                .Include(adc => adc.AdiTeachers)
                .ThenInclude(at => at.Teacher)
                .AsNoTracking()
                .OrderBy(adc => adc.DateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching academic development classes for school {SchoolId}.", schoolId);
            return [];
        }
    }

    /// <summary>
    /// Get ADI classes by school, filtered by teacher ID.
    /// For regular teachers, returns only their own ADI classes.
    /// Includes both Support ADIs where TeacherId matches, and Break ADIs where teacher is in AdiTeachers.
    /// </summary>
    public async Task<List<AcademicDevelopmentClass>> GetBySchoolAndTeacherAsync(Guid schoolId, Guid teacherId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            
            return await context.AcademicDevelopmentClasses
                .Where(adc => adc.SchoolId == schoolId && 
                              adc.AcademicYear != null && adc.AcademicYear.IsCurrent &&
                              (adc.TeacherId == teacherId || adc.AdiTeachers.Any(at => at.TeacherId == teacherId)))
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .Include(adc => adc.AdiSubjects)
                .ThenInclude(asub => asub.Subject)
                .Include(adc => adc.AdiTeachers)
                .ThenInclude(at => at.Teacher)
                .AsNoTracking()
                .OrderByDescending(adc => adc.DateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching academic development classes for school {SchoolId} and teacher {TeacherId}.", schoolId, teacherId);
            return [];
        }
    }

    public async Task<AcademicDevelopmentClass?> GetByIdAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.AcademicDevelopmentClasses
                .Include(adc => adc.SchoolGrade)
                .ThenInclude(sg => sg!.SystemGrade)
                .Include(adc => adc.Subject)
                .Include(adc => adc.Teacher)
                .Include(adc => adc.School)
                .Include(adc => adc.AdiLearners)
                .ThenInclude(al => al.Learner)
                .Include(adc => adc.AdiSubjects)
                .ThenInclude(asub => asub.Subject)
                .Include(adc => adc.AdiTeachers)
                .ThenInclude(at => at.Teacher)
                .AsNoTracking()
                .FirstOrDefaultAsync(adc => adc.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching academic development class with ID: {Id}", id);
            return null;
        }
    }

    public async Task<AcademicDevelopmentClass> CreateAsync(AcademicDevelopmentClass academicDevelopmentClass)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            academicDevelopmentClass.Id = Guid.NewGuid();
            
            // Get current academic year for the school
            academicDevelopmentClass.AcademicYearId = await schoolService.GetCurrentAcademicYearIdAsync(academicDevelopmentClass.SchoolId);
            
            // Ensure DateTime is in UTC
            if (academicDevelopmentClass.DateTime.Kind == DateTimeKind.Unspecified)
            {
                academicDevelopmentClass.DateTime = DateTime.SpecifyKind(academicDevelopmentClass.DateTime, DateTimeKind.Utc);
            }
            else if (academicDevelopmentClass.DateTime.Kind == DateTimeKind.Local)
            {
                academicDevelopmentClass.DateTime = academicDevelopmentClass.DateTime.ToUniversalTime();
            }

            academicDevelopmentClass.CreatedAt = DateTime.UtcNow;
            academicDevelopmentClass.UpdatedAt = DateTime.UtcNow;

            await context.AcademicDevelopmentClasses.AddAsync(academicDevelopmentClass);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Created new academic development class: {Id}", academicDevelopmentClass.Id);
            return academicDevelopmentClass;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating academic development class.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(AcademicDevelopmentClass academicDevelopmentClass)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.AcademicDevelopmentClasses.FindAsync(academicDevelopmentClass.Id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent academic development class {Id}.", academicDevelopmentClass.Id);
                return false;
            }

            existing.DateTime = academicDevelopmentClass.DateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(academicDevelopmentClass.DateTime, DateTimeKind.Utc)
                : academicDevelopmentClass.DateTime.Kind == DateTimeKind.Local 
                    ? academicDevelopmentClass.DateTime.ToUniversalTime()
                    : academicDevelopmentClass.DateTime;
            existing.SchoolGradeId = academicDevelopmentClass.SchoolGradeId;
            existing.SubjectId = academicDevelopmentClass.SubjectId;
            existing.TeacherId = academicDevelopmentClass.TeacherId;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = academicDevelopmentClass.UpdatedBy;

            context.Entry(existing).State = EntityState.Modified;
            await context.SaveChangesAsync();
            
            logger.LogInformation("Updated academic development class: {Id}", academicDevelopmentClass.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating academic development class: {Id}", academicDevelopmentClass.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.AcademicDevelopmentClasses.FindAsync(id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to delete non-existent academic development class {Id}.", id);
                return false;
            }

            context.AcademicDevelopmentClasses.Remove(existing);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Deleted academic development class: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting academic development class: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Start attendance tracking for an ADI class.
    /// Sets IsAttendanceOpen to true and records the start time.
    /// </summary>
    public async Task<bool> StartAttendanceAsync(Guid id, Guid? userId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var adi = await context.AcademicDevelopmentClasses.FindAsync(id);

            if (adi == null)
            {
                logger.LogWarning("Attempted to start attendance for non-existent ADI class {Id}.", id);
                return false;
            }

            adi.IsAttendanceOpen = true;
            adi.AttendanceStartedAt = DateTime.UtcNow;
            adi.AttendanceStoppedAt = null; // Clear any previous stop time
            adi.UpdatedAt = DateTime.UtcNow;
            adi.UpdatedBy = userId;

            context.Entry(adi).State = EntityState.Modified;
            await context.SaveChangesAsync();

            logger.LogInformation("Started attendance for ADI class {Id} at {Time}", id, adi.AttendanceStartedAt);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting attendance for ADI class {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Stop attendance tracking for an ADI class.
    /// Sets IsAttendanceOpen to false and records the stop time.
    /// Learners arriving after this time will be marked as late.
    /// </summary>
    public async Task<bool> StopAttendanceAsync(Guid id, Guid? userId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var adi = await context.AcademicDevelopmentClasses.FindAsync(id);

            if (adi == null)
            {
                logger.LogWarning("Attempted to stop attendance for non-existent ADI class {Id}.", id);
                return false;
            }

            adi.IsAttendanceOpen = false;
            adi.AttendanceStoppedAt = DateTime.UtcNow;
            adi.UpdatedAt = DateTime.UtcNow;
            adi.UpdatedBy = userId;

            context.Entry(adi).State = EntityState.Modified;
            await context.SaveChangesAsync();

            logger.LogInformation("Stopped attendance for ADI class {Id} at {Time}", id, adi.AttendanceStoppedAt);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping attendance for ADI class {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Get the current attendance status of an ADI class.
    /// </summary>
    public async Task<(bool IsOpen, DateTime? StartedAt, DateTime? StoppedAt)> GetAttendanceStatusAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var adi = await context.AcademicDevelopmentClasses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adi == null)
            {
                return (false, null, null);
            }

            return (adi.IsAttendanceOpen, adi.AttendanceStartedAt, adi.AttendanceStoppedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting attendance status for ADI class {Id}", id);
            return (false, null, null);
        }
    }
}
