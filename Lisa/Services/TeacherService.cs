using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lisa.Services;

public class TeacherService(IDbContextFactory<LisaDbContext> dbContextFactory, IUiEventService uiEventService, ILogger<TeacherService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly IUiEventService _uiEventService = uiEventService;
    private readonly ILogger<TeacherService> _logger = logger;

    /// <summary>
    /// Retrieves all teachers.
    /// </summary>
    public async Task<List<Teacher>> GetAllAsync()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Teachers
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all teachers.");
            return [];
        }
    }

    /// <summary>
    /// Creates a new teacher.
    /// </summary>
    public async Task<bool> CreateAsync(Teacher teacher)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.Teachers.AddAsync(teacher);
            await context.SaveChangesAsync();
            await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
            _logger.LogInformation("Created new teacher: {TeacherId}", teacher.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher.");
            return false;
        }
    }

    /// <summary>
    /// Retrieves a teacher by ID with related data.
    /// </summary>
    public async Task<Teacher?> GetByIdAsync(Guid id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Teachers
                .AsNoTracking()
                .Include(t => t.School!)
                .Include(t => t.Subjects!)
                .Include(t => t.RegisterClasses!)
                    .ThenInclude(rc => rc.Grade!)
                .Include(t => t.Periods!)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teacher with ID: {TeacherId}", id);
            return null;
        }
    }

    /// <summary>
    /// Updates an existing teacher.
    /// </summary>
    public async Task<bool> UpdateAsync(Teacher teacher)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Teachers.FindAsync(teacher.Id);
            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent teacher. TeacherId: {TeacherId}", teacher.Id);
                return false;
            }

            existing.Surname = teacher.Surname;
            existing.Name = teacher.Name;
            existing.Email = teacher.Email;
            existing.PhoneNumber = teacher.PhoneNumber;
            existing.SchoolId = teacher.SchoolId;

            context.Entry(existing).State = EntityState.Modified;
            await context.SaveChangesAsync();
            await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
            _logger.LogInformation("Updated teacher: {TeacherId}", teacher.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher with ID: {TeacherId}", teacher.Id);
            return false;
        }
    }

    /// <summary>
    /// Checks if a teacher has any assigned register classes.
    /// </summary>
    public async Task<bool> HasRegisterClassesAsync(Guid teacherId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.RegisterClasses.AnyAsync(rc => rc.TeacherId == teacherId);
    }

    /// <summary>
    /// Deletes a teacher by ID.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Teachers.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Attempted to delete non-existent teacher. TeacherId: {TeacherId}", id);
                return false;
            }

            context.Teachers.Remove(existing);
            await context.SaveChangesAsync();
            await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
            _logger.LogInformation("Deleted teacher: {TeacherId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher with ID: {TeacherId}", id);
            return false;
        }
    }

    /// <summary>
    /// Retrieves teachers for a given school.
    /// </summary>
    public async Task<List<Teacher>> GetTeachersForSchoolAsync(Guid schoolId)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Teachers
                .AsNoTracking()
                .Where(t => t.SchoolId == schoolId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teachers for SchoolId: {SchoolId}", schoolId);
            return [];
        }
    }

    /// <summary>
    /// Retrieves available teachers except the given teacher.
    /// </summary>
    public async Task<List<Teacher>> GetAvailableTeachersAsync(Guid teacherId)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var teacher = await context.Teachers.FindAsync(teacherId);
            if (teacher == null) return new List<Teacher>();

            return await context.Teachers
                .AsNoTracking()
                .Where(t => t.SchoolId == teacher.SchoolId && t.Id != teacherId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available teachers for TeacherId: {TeacherId}", teacherId);
            return [];
        }
    }

    /// <summary>
    /// Transfers register classes from one teacher to another.
    /// </summary>
    public async Task<bool> TransferRegisterClassesAsync(Guid oldTeacherId, Guid newTeacherId)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var teachers = await context.Teachers
                .Where(t => t.Id == oldTeacherId || t.Id == newTeacherId)
                .ToListAsync();

            if (teachers.Count < 2) return false;

            var registerClasses = context.RegisterClasses.Where(rc => rc.TeacherId == oldTeacherId);
            await registerClasses.ForEachAsync(rc => rc.TeacherId = newTeacherId);

            await context.SaveChangesAsync();
            _logger.LogInformation("Transferred register classes from TeacherId {OldTeacherId} to TeacherId {NewTeacherId}", oldTeacherId, newTeacherId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring register classes.");
            return false;
        }
    }
}
