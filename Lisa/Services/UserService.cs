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

            // Retrieve users who either belong to the school OR are System Administrators
            var usersQuery = context.Users.AsNoTracking();

            if (SchoolId != null)
            {
                usersQuery = usersQuery.Where(u => u.SchoolId == SchoolId || context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .Contains(Roles.SystemAdministrator));
            }

            var users = await usersQuery
            .Include(t => t.School)
                    .Include(t => t.CareGroups)
                    .Include(t => t.Subjects)
                    .Include(t => t.RegisterClasses)
                    .Include(t => t.Periods)
            .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await (from userRole in context.UserRoles
                                   join role in context.Roles on userRole.RoleId equals role.Id
                                   where userIds.Contains(userRole.UserId)
                                   select new { userRole.UserId, role.Name })
                                .ToListAsync();

            foreach (var user in users)
            {
                user.Roles = userRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.Name)
                    .ToList();
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
    /// Retrieves a teacher by ID with related data.
    /// </summary>
    /// <summary>
    /// Retrieves a user by ID with related data (School, CareGroups, Subjects, RegisterClasses, Periods).
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Load user with all the relevant navigations
            var user = await context.Users
                .AsNoTracking()
                .Include(u => u.School)
                .Include(u => u.CareGroups)
                .Include(u => u.Subjects)
                .Include(u => u.RegisterClasses)!
                    .ThenInclude(rc => rc.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(u => u.Periods)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            // Fetch user roles from the identity tables
            var userRoles = await
                (from userRole in context.UserRoles
                 join role in context.Roles on userRole.RoleId equals role.Id
                 where userRole.UserId == id
                 select role.Name)
                .ToListAsync();

            user.Roles = userRoles;

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new teacher.
    /// </summary>
    public async Task<bool> CreateAsync(UserViewModel user)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            if (await context.Users.AnyAsync(t => t.Email == user.Email))
            {
                _logger.LogError("Duplicate email detected: {Email}", user.Email);
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                _logger.LogError("Password is required for new teacher: {Email}", user.Email);
                return false;
            }

            var existingCareGroups = await context.CareGroups
                .Where(cg => user.SelectedCareGroupIds.Contains(cg.Id))
                .ToListAsync();

            var teacherEntity = new User
            {
                Surname = user.Surname,
                Abbreviation = user.Abbreviation,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                SchoolId = user.SchoolId,
                CareGroups = existingCareGroups,
                UserName = user.Email,
            };

            teacherEntity.PasswordHash = _passwordHasher.HashPassword(teacherEntity, user.Password);

            context.Users.Add(teacherEntity);
            await context.SaveChangesAsync();

            await _uiEventService.PublishAsync(UiEvents.UsersUpdated);
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
    public async Task<bool> UpdateAsync(User user, IEnumerable<Guid> selectedCareGroupIds, string? newPassword)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Users
                .Include(t => t.CareGroups)
                .FirstOrDefaultAsync(t => t.Id == user.Id);

            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent teacher. TeacherId: {TeacherId}", user.Id);
                return false;
            }

            if (existing.Email != user.Email)
            {
                var emailExists = await context.Users.AnyAsync(t => t.Email == user.Email && t.Id != user.Id);
                if (emailExists)
                {
                    _logger.LogWarning("Attempted to update teacher with duplicate email: {Email}", user.Email);
                    return false;
                }
            }

            existing.Surname = user.Surname;
            existing.Abbreviation = user.Abbreviation;
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.PhoneNumber = user.PhoneNumber;
            existing.SchoolId = user.SchoolId;

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
                _logger.LogInformation("Updated password for teacher: {TeacherId}", user.Id);
            }

            await context.SaveChangesAsync();
            await _uiEventService.PublishAsync(UiEvents.UsersUpdated);
            _logger.LogInformation("Updated teacher: {TeacherId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher with ID: {TeacherId}", user.Id);
            return false;
        }
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Users.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Attempted to delete non-existent teacher. TeacherId: {TeacherId}", id);
                return false;
            }

            context.Users.Remove(existing);
            await context.SaveChangesAsync();
            await _uiEventService.PublishAsync(UiEvents.UsersUpdated);
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
    /// Retrieves all users.
    /// </summary>
    public async Task<List<User>> GetAllAsync()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Users
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
    /// Checks if a teacher has any assigned register classes.
    /// </summary>
    public async Task<bool> HasRegisterClassesAsync(Guid userId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.RegisterClasses.AnyAsync(rc => rc.UserId == userId);
    }

    /// <summary>
    /// Retrieves available teachers except the given teacher.
    /// </summary>
    public async Task<List<User>> GetAvailableTeachersAsync(Guid userId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var teacher = await context.Users.FindAsync(userId);
            if (teacher == null) return new List<User>();

            return await context.Users
                .AsNoTracking()
                .Where(t => t.SchoolId == teacher.SchoolId && t.Id != userId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available users for TeacherId: {TeacherId}", userId);
            return [];
        }
    }

    /// <summary>
    /// Transfers register classes from one teacher to another.
    /// </summary>
    public async Task<bool> TransferRegisterClassesAsync(Guid oldUserId, Guid newUserId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var teachers = await context.Users
                .Where(t => t.Id == oldUserId || t.Id == newUserId)
                .ToListAsync();

            if (teachers.Count < 2) return false;

            var registerClasses = context.RegisterClasses.Where(rc => rc.UserId == oldUserId);
            await registerClasses.ForEachAsync(rc => rc.UserId = newUserId);

            await context.SaveChangesAsync();
            _logger.LogInformation(
                "Transferred register classes from TeacherId {OldTeacherId} to TeacherId {NewTeacherId}", oldUserId,
                newUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring register classes.");
            return false;
        }
    }
}
