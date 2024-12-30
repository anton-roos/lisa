using Microsoft.AspNetCore.Identity;

namespace Lisa.Data;

public static class DatabaseSeed
{
    const string AdminEmail = "admin@dcegroup.co.za";
    const string AdminPassword = "Lis@Adm!n7Dc3Gr0up";

    public static async Task SeedAdmin(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            var user = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail
            };
            await userManager.CreateAsync(user, AdminPassword);
        }

        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, Roles.SystemAdministrator))
        {
            await userManager.AddToRoleAsync(adminUser, Roles.SystemAdministrator);
        }
    }


    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roles = new[]
        {
            Roles.SystemAdministrator,
            Roles.Principal,
            Roles.Administrator,
            Roles.SchoolManagement,
            Roles.Teacher
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        
    }
}