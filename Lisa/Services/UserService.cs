using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;
public class UserService(UserManager<User> userManager, IDbContextFactory<LisaDbContext> dbContextFactory, IHttpContextAccessor httpContextAccessor)
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Guid?> GetLoggedInUserIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
        {
            return null;
        }

        var user = await _userManager.GetUserAsync(httpContext.User);
        return user?.Id;
    }

    public async Task<List<TUser>> GetAllByRoleAndSchoolAsync<TUser>(Guid? schoolId = null) where TUser : User
    {
        var _context = await _dbContextFactory.CreateDbContextAsync();
        IQueryable<TUser> query = _context.Users.OfType<TUser>();

        switch (typeof(TUser))
        {
            case Type t when t == typeof(Principal):
                query = query.Cast<Principal>().Include(p => p.School).Cast<TUser>();
                break;
            case Type t when t == typeof(Administrator):
                query = query.Cast<Administrator>().Include(a => a.School).Cast<TUser>();
                break;
            case Type t when t == typeof(SchoolManagement):
                query = query.Cast<SchoolManagement>().Include(m => m.School).Cast<TUser>();
                break;
            case Type t when t == typeof(Teacher):
                query = query.Cast<Teacher>().Include(t => t.School).Cast<TUser>();
                break;
            case Type t when t == typeof(Learner):
                query = query.Cast<Learner>().Include(l => l.School).Cast<TUser>();
                break;
        }

        var users = await query.ToListAsync();

        if (schoolId != null)
        {
            return users.Where(u => u.SchoolId == schoolId).ToList();
        }
        else
        {
            return await _context.Users.Where(u => u.UserType == "User").ToListAsync() as List<TUser> ?? [];
        }
    }

    public async Task<TUser> GetByIdAsync<TUser>(Guid id) where TUser : User
    {

        var _context = await _dbContextFactory.CreateDbContextAsync();
        var user = await _context.Users.OfType<TUser>().FirstOrDefaultAsync(u => u.Id == id);
        await _context.DisposeAsync();
        if (user == null)
        {
            throw new Exception("User not found");
        }
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

    public async Task UpdateUserPassword(User user, string password)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _userManager.ResetPasswordAsync(user, token, password);
    }

    public async Task DeleteAsync<TUser>(Guid id) where TUser : User
    {
        var user = await GetByIdAsync<TUser>(id);
        if (user == null) return;

        await _userManager.DeleteAsync(user);
    }
}
