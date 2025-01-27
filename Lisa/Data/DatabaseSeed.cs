using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        await ApplyMigrations(serviceProvider);
    }

    private static async Task ApplyMigrations(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<LisaDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

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
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task SeedAdmin(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            var user = new User
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FirstName = "System",
                LastName = "Admin",
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
                new Subject {
                    Id = 1,
                    Name = "English HL (RRR - 12)",
                    Code = "Eng HL",
                    Description = "English Home Language",
                    Order = 1,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 2,
                    Name = "Afrikaans FAL (RRR - 12)",
                    Code = "Afr FAL",
                    Description = "Afrikaans First Additional Language",
                    Order = 2,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 3,
                    Name = "Life Skills (RRR - 6)",
                    Code = "LSK",
                    Description = "Life Skills",
                    Order = 3,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 4,
                    Name = "Life Orientation (7 - 12)",
                    Code = "LO",
                    Description = "Life Orientation",
                    Order = 4,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 5,
                    Name = "Mathematics (RRR - 9)",
                    Code = "MATF",
                    Description = "Mathematics",
                    Order = 5,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 6,
                    Name = "Mathematics (10 - 12)",
                    Code = "MATH",
                    Description = "Mathematics",
                    Order = 6,
                    SubjectType = SubjectType.MathCombination
                },

                new Subject {
                    Id = 7,
                    Name = "Mathematical Literacy (10 - 12)",
                    Code = "MATL",
                    Description = "Mathematical Literacy",
                    Order = 7,
                    SubjectType = SubjectType.MathCombination
                },

                new Subject {
                    Id = 8,
                    Name = "Natural Sciences (4 - 9)",
                    Code = "NS",
                    Description = "Natural Sciences",
                    Order = 8,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 9,
                    Name = "Physical Sciences (10 - 12)",
                    Code = "PS",
                    Description = "Physical Sciences",
                    Order = 9,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 10,
                    Name = "Life Sciences (10 - 12)",
                    Code = "LSC",
                    Description = "Life Sciences",
                    Order = 10,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 11,
                    Name = "EMS Business (8 - 9)",
                    Code = "EMS BS",
                    Description = "Economic and Management Sciences - Business",
                    Order = 11,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 12,
                    Name = "EMS Accounting (8 - 9)",
                    Code = "EMS AC",
                    Description = "Economic and Management Sciences - Accounting",
                    Order = 12,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 13,
                    Name = "Business Studies (10 - 12)",
                    Code = "BUS",
                    Description = "Business Studies",
                    Order = 13,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 14,
                    Name = "Accounting (10 - 12)",
                    Code = "ACC",
                    Description = "Accounting",
                    Order = 14,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 15,
                    Name = "Economics (10 - 12)",
                    Code = "ECO",
                    Description = "Economics",
                    Order = 15,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 16,
                    Name = "Tourism (10 - 12)",
                    Code = "TOUR",
                    Description = "Tourism",
                    Order = 16,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 17,
                    Name = "SS History (4 - 9)",
                    Code = "SS HIS",
                    Description = "Social Sciences - History",
                    Order = 17,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 18,
                    Name = "SS Geography (4 - 9)",
                    Code = "SS GEO",
                    Description = "Social Sciences - Geography",
                    Order = 18,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 19,
                    Name = "History (10 - 12)",
                    Code = "HIS",
                    Description = "History",
                    Order = 19,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 20,
                    Name = "Geography (10 - 12)",
                    Code = "GEO",
                    Description = "Geography",
                    Order = 20,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 21,
                    Name = "Creative Arts (4 - 9)",
                    Code = "CA",
                    Description = "Creative Arts",
                    Order = 21,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 22,
                    Name = "Technology (7 - 9)",
                    Code = "TECH",
                    Description = "Technology",
                    Order = 22,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 23,
                    Name = "CAT (10 - 12)",
                    Code = "CAT",
                    Description = "Computer Applications Technology",
                    Order = 23,
                    SubjectType = SubjectType.Combination
                }
            ]);

            await dbContext.SaveChangesAsync();
        }
    }
}
