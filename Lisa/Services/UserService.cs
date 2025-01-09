using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;
public class UserService(UserManager<User> userManager, IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<List<User>> GetUsersByRoleAndSchoolAsync(string roleName, Guid? schoolId = null)
    {
        var allUsers = await _userManager.GetUsersInRoleAsync(roleName);

        switch (roleName)
        {
            case Roles.Principal:
                {
                    var principals = allUsers
                        .OfType<Principal>()
                        .Where(p => p.SchoolId == schoolId)
                        .ToList();

                    return principals.Cast<User>().ToList();
                }

            case Roles.Administrator:
                {
                    var admins = allUsers
                        .OfType<Administrator>()
                        .Where(a => a.SchoolId == schoolId)
                        .ToList();

                    return admins.Cast<User>().ToList();
                }

            case Roles.SystemAdministrator:
                {
                    var sysAdmins = allUsers
                        .OfType<User>()
                        .ToList();

                    return sysAdmins.Cast<User>().ToList();
                }

            case Roles.SchoolManagement:
                {
                    var managementUsers = allUsers
                        .OfType<SchoolManagement>()
                        .Where(m => m.SchoolId == schoolId)
                        .ToList();

                    return managementUsers.Cast<User>().ToList();
                }

            default:
                {
                    var baseUsers = allUsers
                        .ToList();
                    return baseUsers.Cast<User>().ToList();
                }
        }
    }

    public async Task<TUser> GetByIdAsync<TUser>(Guid id) where TUser : User
    {

        var _context = await _dbContextFactory.CreateDbContextAsync();
        var user = await _context.Users.OfType<TUser>().FirstOrDefaultAsync(u => u.Id == id);
        await _context.DisposeAsync();
        return user;
    }

    public async Task AddUser<TUser>(TUser user, string password)
    {
        switch (user)
        {
            case Principal principal:
                {
                    await _userManager.CreateAsync(principal, password);
                    await _userManager.AddToRoleAsync(principal, Roles.Principal);
                    break;
                }

            case Administrator admin:
                {
                    await _userManager.CreateAsync(admin, password);
                    await _userManager.AddToRoleAsync(admin, Roles.Administrator);
                    break;
                }

            case SchoolManagement management:
                {
                    await _userManager.CreateAsync(management, password);
                    await _userManager.AddToRoleAsync(management, Roles.SchoolManagement);
                    break;
                }

            case SystemAdministrator sysAdmin:
                {
                    await _userManager.CreateAsync(sysAdmin, password);
                    await _userManager.AddToRoleAsync(sysAdmin, Roles.SystemAdministrator);
                    break;
                }
            case Teacher teacher:
                {
                    var result = await _userManager.CreateAsync(teacher, password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create user: {errors}");
                    }
                    await _userManager.AddToRoleAsync(teacher, Roles.Teacher);
                    break;
                }
            case User baseUser:
                {
                    await _userManager.CreateAsync(baseUser, password);
                    await _userManager.AddToRoleAsync(baseUser, Roles.SystemAdministrator);
                    break;
                }
        }
    }

    public async Task DeleteAsync<TUser>(Guid id) where TUser : User
    {
        var user = await GetByIdAsync<TUser>(id);
        if (user == null) return;

        await _userManager.DeleteAsync(user);
    }
}