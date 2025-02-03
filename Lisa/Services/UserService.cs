using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class UserService(
    UserManager<User> userManager,
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<UserService> logger,
    IUiEventService uiEventService,
    IPasswordHasher<User> passwordHasher)
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<UserService> _logger = logger;
    private readonly IUiEventService _uiEventService = uiEventService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    /// <summary>
    /// Retrieves all users by role and school.
    /// </summary>
    public async Task<List<User>> GetAllByRoleAndSchoolAsync(string[] roles, Guid? SchoolId = null)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var usersQuery = context.Users
                .AsNoTracking()
                .Where(u => SchoolId == null || u.SchoolId == SchoolId);

            // Include CareGroups only for Teachers
            if (roles.Contains(Roles.Teacher))
            {
                usersQuery = usersQuery.OfType<Teacher>().Include(t => t.CareGroups);
            }

            var users = await usersQuery.ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            var userRoles = await (from userRole in context.UserRoles
                                   join role in context.Roles on userRole.RoleId equals role.Id
                                   where userIds.Contains(userRole.UserId)
                                   select new { userRole.UserId, role.Name })
                                .ToListAsync();

            foreach (var user in users)
            {
                user.Roles = [.. userRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.Name)];
            }

            return users.Where(u => u.Roles.Intersect(roles).Any()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users for SchoolId {SchoolId}.", SchoolId);
            return [];
        }
    }

    /// <summary>
    /// Retrieves all teachers.
    /// </summary>
    public async Task<List<Teacher>> GetAllTeachersAsync()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Teachers.AsNoTracking().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all teachers.");
            return [];
        }
    }

    /// <summary>
    /// Retrieves a teacher by ID with related data.
    /// </summary>
    public async Task<Teacher?> GetTeacherByIdAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Teachers
                .AsNoTracking()
                .Include(t => t.School)
                .Include(t => t.Subjects)
                .Include(t => t.RegisterClasses)
                    .ThenInclude(rc => rc.SchoolGrade)
                    .ThenInclude(sg => sg.SystemGrade)
                .Include(t => t.Periods)
                .Include(t => t.CareGroups)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teacher with ID: {TeacherId}", id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new teacher.
    /// </summary>
    public async Task<bool> CreateTeacherAsync(TeacherViewModel teacher)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            if (await context.Teachers.AnyAsync(t => t.Email == teacher.Email))
            {
                _logger.LogError("Duplicate email detected: {Email}", teacher.Email);
                return false;
            }

            if (string.IsNullOrWhiteSpace(teacher.Password))
            {
                _logger.LogError("Password is required for new teacher: {Email}", teacher.Email);
                return false;
            }

            var existingCareGroups = await context.CareGroups
                .Where(cg => teacher.SelectedCareGroupIds.Contains(cg.Id))
                .ToListAsync();

            var teacherEntity = new Teacher
            {
                Surname = teacher.Surname,
                Abbreviation = teacher.Abbreviation,
                Name = teacher.Name,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                SchoolId = teacher.SchoolId,
                CareGroups = existingCareGroups,
                UserName = teacher.Email,
            };

            teacherEntity.PasswordHash = _passwordHasher.HashPassword(teacherEntity, teacher.Password);

            context.Teachers.Add(teacherEntity);
            await context.SaveChangesAsync();

            await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
            _logger.LogInformation("Created new teacher: {TeacherId}", teacherEntity.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher.");
            return false;
        }
    }

    /// <summary>
    /// Updates an existing teacher.
    /// </summary>
    public async Task<bool> UpdateTeacherAsync(Teacher teacher, IEnumerable<Guid> selectedCareGroupIds, string? newPassword)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Teachers
                .Include(t => t.CareGroups)
                .FirstOrDefaultAsync(t => t.Id == teacher.Id);

            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent teacher. TeacherId: {TeacherId}", teacher.Id);
                return false;
            }

            if (existing.Email != teacher.Email)
            {
                var emailExists = await context.Teachers.AnyAsync(t => t.Email == teacher.Email && t.Id != teacher.Id);
                if (emailExists)
                {
                    _logger.LogWarning("Attempted to update teacher with duplicate email: {Email}", teacher.Email);
                    return false;
                }
            }

            existing.Surname = teacher.Surname;
            existing.Abbreviation = teacher.Abbreviation;
            existing.Name = teacher.Name;
            existing.Email = teacher.Email;
            existing.PhoneNumber = teacher.PhoneNumber;
            existing.SchoolId = teacher.SchoolId;

            var existingCareGroups = await context.CareGroups
                .Where(cg => selectedCareGroupIds.Contains(cg.Id))
                .ToListAsync();

            existing.CareGroups.Clear();
            foreach (var careGroup in existingCareGroups)
            {
                context.CareGroups.Attach(careGroup);
                existing.CareGroups.Add(careGroup);
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                existing.PasswordHash = _passwordHasher.HashPassword(existing, newPassword);
                _logger.LogInformation("Updated password for teacher: {TeacherId}", teacher.Id);
            }

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
    /// Deletes a teacher.
    /// </summary>
    public async Task<bool> DeleteTeacherAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
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
    /// Retrieves all teachers.
    /// </summary>
    public async Task<List<Teacher>> GetAllAsync()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
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

    public async Task<bool> CreateAsync(TeacherViewModel teacher)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            if (await context.Teachers.AnyAsync(t => t.Email == teacher.Email))
            {
                _logger.LogError("Duplicate email detected: {Email}", teacher.Email);
                return false;
            }

            if (string.IsNullOrWhiteSpace(teacher.Password))
            {
                _logger.LogError("Password is required for new teacher: {Email}", teacher.Email);
                return false;
            }

            var existingCareGroups = await context.CareGroups
                .Where(cg => teacher.SelectedCareGroupIds.Contains(cg.Id))
                .ToListAsync();

            var teacherEntity = new Teacher
            {
                Surname = teacher.Surname,
                Abbreviation = teacher.Abbreviation,
                Name = teacher.Name,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                SchoolId = teacher.SchoolId,
                CareGroups = existingCareGroups,
                UserName = teacher.Email,
            };

            teacherEntity.PasswordHash = _passwordHasher.HashPassword(teacherEntity, teacher.Password);

            context.Teachers.Add(teacherEntity);
            await context.SaveChangesAsync();

            await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
            _logger.LogInformation("Created new teacher: {TeacherId}", teacherEntity.Id);

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
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Teachers
                .AsNoTracking()
                .Include(t => t.School!)
                .Include(t => t.Subjects!)
                .Include(t => t.RegisterClasses!)
                .ThenInclude(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade!)
                .Include(t => t.Periods!)
                .Include(t => t.CareGroups!)
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
    public async Task<bool> UpdateAsync(Teacher teacher, IEnumerable<Guid> selectedCareGroupIds, string? newPassword)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Teachers
                .Include(t => t.CareGroups)
                .FirstOrDefaultAsync(t => t.Id == teacher.Id);

            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent teacher. TeacherId: {TeacherId}", teacher.Id);
                return false;
            }

            if (existing.Email != teacher.Email)
            {
                var emailExists = await context.Teachers.AnyAsync(t => t.Email == teacher.Email && t.Id != teacher.Id);
                if (emailExists)
                {
                    _logger.LogWarning("Attempted to update teacher with duplicate email: {Email}", teacher.Email);
                    return false;
                }
            }

            existing.Surname = teacher.Surname;
            existing.Abbreviation = teacher.Abbreviation;
            existing.Name = teacher.Name;
            existing.Email = teacher.Email;
            existing.PhoneNumber = teacher.PhoneNumber;
            existing.SchoolId = teacher.SchoolId;

            var existingCareGroups = await context.CareGroups
                .Where(cg => selectedCareGroupIds.Contains(cg.Id))
                .ToListAsync();

            existing.CareGroups.Clear();
            foreach (var careGroup in existingCareGroups)
            {
                context.CareGroups.Attach(careGroup);
                existing.CareGroups.Add(careGroup);
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                existing.PasswordHash = _passwordHasher.HashPassword(existing, newPassword);
                _logger.LogInformation("Updated password for teacher: {TeacherId}", teacher.Id);
            }

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
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.RegisterClasses.AnyAsync(rc => rc.TeacherId == teacherId);
    }

    /// <summary>
    /// Deletes a teacher by ID.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
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
            using var context = await _dbContextFactory.CreateDbContextAsync();
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
            using var context = await _dbContextFactory.CreateDbContextAsync();
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
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var teachers = await context.Teachers
                .Where(t => t.Id == oldTeacherId || t.Id == newTeacherId)
                .ToListAsync();

            if (teachers.Count < 2) return false;

            var registerClasses = context.RegisterClasses.Where(rc => rc.TeacherId == oldTeacherId);
            await registerClasses.ForEachAsync(rc => rc.TeacherId = newTeacherId);

            await context.SaveChangesAsync();
            _logger.LogInformation(
                "Transferred register classes from TeacherId {OldTeacherId} to TeacherId {NewTeacherId}", oldTeacherId,
                newTeacherId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring register classes.");
            return false;
        }
    }
}
