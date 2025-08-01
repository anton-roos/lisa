using Lisa.Data;
using Lisa.Interfaces;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class UserService(
    UserManager<User> userManager,
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<UserService> logger,
    UiEventService uiEventService,
    IPasswordHasher<User> passwordHasher
) : IUserService
{
    public async Task<List<User>> GetAllByRoleAndSchoolAsync(string[] roles, Guid? schoolId)
    {
        try
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var usersQuery = context.Users.AsNoTracking();

            if (schoolId != null)
            {
                usersQuery = usersQuery.Where(u => u.SchoolId == schoolId || context.UserRoles
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
                user.Roles =
                [
                    .. userRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Select(ur => ur.Name)
                ];
            }

            return [.. users.Where(u => u.Roles.Intersect(roles).Any())];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching users for SchoolId {SchoolId}.", schoolId);
            return [];
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

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
            {
                return null;
            }

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
            logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            return null;
        }
    }

    public async Task<User?> GetTeacherForGradeAndSubjectAsync(Guid? schoolId, Guid gradeId, int subjectId)
    {
        Guard.Against.Default(gradeId, nameof(gradeId), "Grade ID cannot be an empty GUID");

        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var grade = await context.SchoolGrades.FindAsync(gradeId);

            var user = await context.Users
                .Include(u => u.Subjects)
                .FirstOrDefaultAsync(u => u.Subjects != null
                                          && u.SchoolId == schoolId
                                          && grade != null
                                          && u.Subjects
                                              .Any(ts => ts.Grade == grade.SystemGrade.SequenceNumber
                                                         && ts.SubjectId == subjectId));

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching teacher for grade: {GradeId} and subject: {SubjectId}", gradeId,
                subjectId);
            return null;
        }
    }

    public async Task UpdateUserSelectedSchool(User user)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.Users.FindAsync(user.Id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent teacher. TeacherId: {TeacherId}", user.Id);
                return;
            }

            Guard.Against.Null(user, nameof(user));

            existing.SchoolId = user.SchoolId;

            await context.SaveChangesAsync();
            logger.LogInformation("Updated teacher's school: {TeacherId}", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating teacher's school with ID: {TeacherId}", user.Id);
        }
    }

    public async Task<bool> UpdateAsync(UserViewModel user, string? newPassword)
    {
        Guard.Against.Null(user, nameof(user));

        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.Users
                .Include(t => t.CareGroups)
                .Include(t => t.Subjects)
                .FirstOrDefaultAsync(t => t.Id == user.Id);

            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent teacher. TeacherId: {TeacherId}", user.Id);
                return false;
            }

            existing.Surname = user.Surname;
            existing.Abbreviation = user.Abbreviation;
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.SchoolId = user.SchoolId;

            existing.CareGroups?.Clear();
            if (user.SelectedCareGroupIds.Count != 0)
            {
                var newCareGroups = await context.CareGroups
                    .Where(cg => user.SelectedCareGroupIds.Contains(cg.Id))
                    .ToListAsync();
                foreach (var careGroup in newCareGroups)
                {
                    existing.CareGroups?.Add(careGroup);
                }
            }

            existing.Subjects?.Clear();
            if (user.Subjects != null && user.Subjects.Any())
            {
                foreach (var teacherSubject in user.Subjects)
                {
                    existing?.Subjects?.Add(teacherSubject);
                }
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                Guard.Against.Null(existing, nameof(existing), "User cannot be null when updating password");
                existing.PasswordHash = passwordHasher.HashPassword(existing, newPassword);
                logger.LogInformation("Updated password for teacher: {TeacherId}", user.Id);
            }

            await context.SaveChangesAsync();

            Guard.Against.Null(existing, nameof(existing), $"User no longer exists after save. UserId: {user.Id}");

            var currentRoles = await userManager.GetRolesAsync(existing);

            var rolesToAdd = user.SelectedRoles.Except(currentRoles).ToArray();
            var rolesToRemove = currentRoles.Except(user.SelectedRoles).ToArray();

            if (rolesToRemove.Length != 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(existing, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to remove roles: {ErrorMessage}", errorMessage);
                    return false;
                }
            }

            if (rolesToAdd.Length != 0)
            {
                var addResult = await userManager.AddToRolesAsync(existing, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to add roles: {ErrorMessage}", errorMessage);
                    return false;
                }
            }

            await uiEventService.PublishAsync(UiEvents.UsersUpdated, null);
            logger.LogInformation("Updated teacher: {TeacherId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating teacher with ID: {TeacherId}", user.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Guard.Against.Default(id, nameof(id), "User ID cannot be an empty GUID");

        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var existing = await context.Users.FindAsync(id);
            if (existing == null)
            {
                logger.LogWarning("Attempted to delete non-existent teacher. TeacherId: {TeacherId}", id);
                return false;
            }

            context.Users.Remove(existing);
            await context.SaveChangesAsync();
            await uiEventService.PublishAsync(UiEvents.UsersUpdated, null);
            logger.LogInformation("Deleted teacher: {TeacherId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting teacher with ID: {TeacherId}", id);
            return false;
        }
    }

    public async Task<List<User>> GetBySchoolAsync(Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Users
                .Where(u => u.SchoolId == schoolId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all teachers.");
            return [];
        }
    }

    public async Task<bool> HasRegisterClassesAsync(Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.RegisterClasses.AnyAsync(rc => rc.UserId == userId);
    }

    public async Task<List<User>> GetAvailableTeachersAsync(Guid userId)
    {
        Guard.Against.Default(userId, nameof(userId), "User ID cannot be an empty GUID");

        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var teacher = await context.Users.FindAsync(userId);

            if (teacher == null)
            {
                logger.LogWarning("Teacher with ID {TeacherId} not found", userId);
                return [];
            }

            return await context.Users
                .AsNoTracking()
                .Where(t => t.SchoolId == teacher.SchoolId && t.Id != userId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching available users for TeacherId: {TeacherId}", userId);
            return [];
        }
    }

    public async Task<bool> TransferRegisterClassesAsync(Guid oldUserId, Guid newUserId)
    {
        Guard.Against.Default(oldUserId, nameof(oldUserId), "Old user ID cannot be an empty GUID");
        Guard.Against.Default(newUserId, nameof(newUserId), "New user ID cannot be an empty GUID");

        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var teachers = await context.Users
                .Where(t => t.Id == oldUserId || t.Id == newUserId)
                .ToListAsync();

            if (teachers.Count < 2)
            {
                return false;
            }

            var registerClasses = context.RegisterClasses.Where(rc => rc.UserId == oldUserId);
            await registerClasses.ForEachAsync(rc => rc.UserId = newUserId);

            await context.SaveChangesAsync();
            logger.LogInformation(
                "Transferred register classes from TeacherId {OldTeacherId} to TeacherId {NewTeacherId}", oldUserId,
                newUserId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error transferring register classes.");
            return false;
        }
    }

    public async Task<List<User>?> GetLearnerPrincipal(Guid learnerId)
    {
        Guard.Against.Default(learnerId, nameof(learnerId), "Learner ID cannot be an empty GUID");

        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var learner = await context.Learners
                .Include(l => l.RegisterClass)
                .ThenInclude(rc => rc!.User)
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            var principals = await GetAllByRoleAndSchoolAsync([Roles.Principal], learner?.SchoolId);

            return principals;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching learner principal for LearnerId: {LearnerId}", learnerId);
            return null;
        }
    }
}
