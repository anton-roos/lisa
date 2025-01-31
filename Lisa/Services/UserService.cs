using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class UserService(UserManager<User> userManager, IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<UserService> logger)
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<UserService> _logger = logger;

    /// <summary>
    /// Retrieves all users of a specific role and optionally filters by school.
    /// </summary>
    public async Task<List<TUser>> GetAllByRoleAndSchoolAsync<TUser>(Guid? schoolId = null) where TUser : User
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var query = context.Users.OfType<TUser>().AsNoTracking();

            if (schoolId != null)
            {
                query = query.Where(u => u.SchoolId == schoolId);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users of type {UserType} for SchoolId {SchoolId}.", typeof(TUser).Name, schoolId);
            return new List<TUser>();
        }
    }

    /// <summary>
    /// Retrieves a user by ID.
    /// </summary>
    public async Task<TUser?> GetByIdAsync<TUser>(Guid id) where TUser : User
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var user = await context.Users.OfType<TUser>().FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID {UserId}.", id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new user with the specified role.
    /// </summary>
    public async Task<IdentityResult> AddUserAsync<TUser>(TUser user, string password) where TUser : User
    {
        try
        {
            IdentityResult result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user {UserId}: {Errors}", user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return result;
            }

            string role = GetRoleForUser(user);
            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("Created user {UserId} with role {Role}.", user.Id, role);

            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {UserId}.", user.Id);
            return IdentityResult.Failed(new IdentityError { Description = "An error occurred while creating the user." });
        }
    }

    /// <summary>
    /// Updates a user's password.
    /// </summary>
    public async Task<IdentityResult> UpdateUserPasswordAsync(User user, string newPassword)
    {
        try
        {
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to update password for UserId {UserId}: {Errors}", user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                _logger.LogInformation("Updated password for UserId {UserId}.", user.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for UserId {UserId}.", user.Id);
            return IdentityResult.Failed(new IdentityError { Description = "An error occurred while updating the password." });
        }
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    public async Task<IdentityResult> DeleteAsync<TUser>(Guid id) where TUser : User
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning("Attempted to delete non-existent user {UserId}.", id);
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete user {UserId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                _logger.LogInformation("Deleted user {UserId}.", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}.", id);
            return IdentityResult.Failed(new IdentityError { Description = "An error occurred while deleting the user." });
        }
    }

    /// <summary>
    /// Determines the role for a given user type.
    /// </summary>
    private static string GetRoleForUser(User user) => user switch
    {
        Principal => Roles.Principal,
        Administrator => Roles.Administrator,
        SchoolManagement => Roles.SchoolManagement,
        Teacher => Roles.Teacher,
        _ => Roles.SystemAdministrator
    };
}
