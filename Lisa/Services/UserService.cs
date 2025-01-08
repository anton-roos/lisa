using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Lisa.Services;
public class UserService(UserManager<User> userManager)
{
    private readonly UserManager<User> _userManager = userManager;

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
}