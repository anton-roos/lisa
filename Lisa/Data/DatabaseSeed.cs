using Microsoft.AspNetCore.Identity;

namespace Lisa.Data;

public static class DatabaseSeed
{
    const string AdminEmail = "admin@dcegroup.co.za";
    const string AdminPassword = "Lis@Adm!n7Dc3Gr0up";

    public static async Task Seed(IServiceProvider serviceProvider)
    {
        await SeedRoles(serviceProvider);
        await SeedAdmin(serviceProvider);
        await SeedSchoolTypes(serviceProvider);
        await SeedSchoolSubjects(serviceProvider);
        await SeedSchoolCurriculum(serviceProvider);
    }

    private static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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

    private static async Task SeedAdmin(IServiceProvider serviceProvider)
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

    private static async Task SeedSchoolTypes(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<LisaDbContext>();

        if (!dbContext.SchoolTypes.Any())
        {
            dbContext.SchoolTypes.AddRange(
            [
                new SchoolType { Name = "Primary School" },
                new SchoolType { Name = "High School" },
                new SchoolType { Name = "Combined School" }
            ]);

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedSchoolCurriculum(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<LisaDbContext>();

        if (!dbContext.SchoolCurriculums.Any())
        {
            dbContext.SchoolCurriculums.AddRange(
            [
                new SchoolCurriculum { Name = "CAPS", Description = "Curriculum Assessment Policy Statements" },
                new SchoolCurriculum { Name = "IEB", Description = "Independent Examinations Board" },
                new SchoolCurriculum { Name = "CIE", Description = "Cambridge International Examinations" },
            ]);

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedSchoolSubjects(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<LisaDbContext>();

        if (!dbContext.Subjects.Any())
        {
            dbContext.Subjects.AddRange(
            [
                new Subject { Name = "Natural Sciences and Technology" },
                new Subject { Name = "Social Sciences" },
                new Subject { Name = "Life Skills" },
                new Subject { Name = "Natural Sciences" },
                new Subject { Name = "Economic and Management Sciences" },
                new Subject { Name = "Technology" },
                new Subject { Name = "Creative Arts" },
                new Subject { Name = "Afrikaans Home Language" },
                new Subject { Name = "English Home Language" },
                new Subject { Name = "isiNdebele Home Language" },
                new Subject { Name = "isiZulu Home Language" },
                new Subject { Name = "isiXhosa Home Language" },
                new Subject { Name = "Sepedi Home Language" },
                new Subject { Name = "Sesotho Home Language" },
                new Subject { Name = "Setswana Home Language" },
                new Subject { Name = "Seswati Home Language" },
                new Subject { Name = "Xitsonga Home Language" },
                new Subject { Name = "Tshivenda Home Language" },
                new Subject { Name = "South African Sign Language Home Language" },
                new Subject { Name = "Afrikaans First Additional Language" },
                new Subject { Name = "English First Additional Language" },
                new Subject { Name = "isiNdebele First Additional Language" },
                new Subject { Name = "isiZulu First Additional Language" },
                new Subject { Name = "isiXhosa First Additional Language" },
                new Subject { Name = "Sepedi First Additional Language" },
                new Subject { Name = "Sesotho First Additional Language" },
                new Subject { Name = "Setswana First Additional Language" },
                new Subject { Name = "Seswati First Additional Language" },
                new Subject { Name = "Xitsonga First Additional Language" },
                new Subject { Name = "Tshivenda First Additional Language" },
                new Subject { Name = "South African Sign Language First Additional Language" },
                new Subject { Name = "Mathematics" },
                new Subject { Name = "Mathematics Literacy" },
                new Subject { Name = "Technical Mathematics" },
                new Subject { Name = "Mechanical Technology" },
                new Subject { Name = "Physical Sciences" },
                new Subject { Name = "Technical Sciences" },
                new Subject { Name = "Agricultural Sciences" },
                new Subject { Name = "Life Sciences" },
                new Subject { Name = "Information Technology" },
                new Subject { Name = "Computer Applications Technology" },
                new Subject { Name = "Engineering Graphics and Design" },
                new Subject { Name = "Civil Technology" },
                new Subject { Name = "Electrical Technology" },
                new Subject { Name = "Agricultural Technology" },
                new Subject { Name = "Geography" },
                new Subject { Name = "History" },
                new Subject { Name = "Accounting" },
                new Subject { Name = "Economics" },
                new Subject { Name = "Business Studies" },
                new Subject { Name = "Consumer Studies" },
                new Subject { Name = "Hospitality Studies" },
                new Subject { Name = "Tourism" },
                new Subject { Name = "Dramatic Arts" },
                new Subject { Name = "Life Orientation" },
                new Subject { Name = "Music" },
                new Subject { Name = "Dance" },
                new Subject { Name = "Visual Arts" },
                new Subject { Name = "Design" },
                new Subject { Name = "Religion Studies" },
                new Subject { Name = "Agricultural Management Practices" },
                new Subject { Name = "Dance Studies Grade" },
                new Subject { Name = "English" },
                new Subject { Name = "English Literature" },
                new Subject { Name = "Science" },
                new Subject { Name = "Physics" },
                new Subject { Name = "Psychology" },
                new Subject { Name = "Computer Science" },
                new Subject { Name = "Modern Foreign Languages" },
                new Subject { Name = "Law" },
                new Subject { Name = "Sociology" },
                new Subject { Name = "Religious Studies" },
                new Subject { Name = "Environmental Management" },
                new Subject { Name = "Chemistry" },
                new Subject { Name = "Biology" },
                new Subject { Name = "Cambridge Global Perspectives" },
                new Subject { Name = "Art and Design" },
                new Subject { Name = "Physical Education" },
                new Subject { Name = "Information and Communication Technology" },
                new Subject { Name = "Foreign Languages" },
                new Subject { Name = "Design and Technology" },
                new Subject { Name = "Drama" },
            ]);

            await dbContext.SaveChangesAsync();
        }
    }
}