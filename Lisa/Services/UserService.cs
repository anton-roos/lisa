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

    public async Task<TUser> GetLoggedInUserAsync<TUser>() where TUser : User
    {
        var user = (_httpContextAccessor.HttpContext?.User != null
            ? await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User)
            : null) 
            ?? throw new Exception("User not found");
        
        var _context = await _dbContextFactory.CreateDbContextAsync();
        var userEntity = await _context.Users.OfType<TUser>().FirstOrDefaultAsync(u => u.Id == user.Id) 
            ?? throw new Exception("User not found in Database");

        await _context.DisposeAsync();
        return userEntity;
    }

    public async Task<List<TUser>> GetAllByRoleAndSchoolAsync<TUser>(Guid? schoolId = null) where TUser : User
    {
        var _context = await _dbContextFactory.CreateDbContextAsync();
        var users = await _context.Users.OfType<TUser>().ToListAsync();
        if (schoolId != null)
        {
            users = users.Where(u => u.SchoolId == schoolId).ToList();
        }
        await _context.DisposeAsync();
        return users;
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