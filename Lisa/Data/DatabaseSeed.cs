using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Data;

public class DatabaseSeed
{
    private const string DefaultAdminEmail = "admin@dcegroup.co.za";
    private static string AdminPassword = "Lis@Adm!n7Dc3Gr0up"; // Securely overridden

    /// <summary>
    /// Runs all seed methods in a single transaction for efficiency.
    /// </summary>
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<DatabaseSeed>>();
        logger.LogInformation("Starting database seeding...");

        try
        {
            await using var dbContext = serviceProvider.GetRequiredService<LisaDbContext>();

            // Apply Migrations First
            await ApplyMigrations(dbContext, logger);

            // Seed Data in a Single Transaction
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await SeedRoles(serviceProvider, logger);
            await SeedAdmin(serviceProvider, logger);
            await SeedSchoolTypes(dbContext, logger);
            await SeedSchoolSubjects(dbContext, logger);
            await SeedSchoolCurriculum(dbContext, logger);
            await transaction.CommitAsync();

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    /// <summary>
    /// Applies pending migrations only if required.
    /// </summary>
    private static async Task ApplyMigrations(LisaDbContext dbContext, ILogger logger)
    {
        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).Any();
        if (pendingMigrations)
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found.");
        }
    }

    /// <summary>
    /// Seeds system roles if they do not exist.
    /// </summary>
    private static async Task SeedRoles(IServiceProvider serviceProvider, ILogger logger)
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
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    /// <summary>
    /// Seeds an admin user with a secure password.
    /// </summary>
    private static async Task SeedAdmin(IServiceProvider serviceProvider, ILogger logger)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        // Fetch Admin Password from Configuration (Environment Variables or appsettings.json)
        var configuredPassword = config["AdminPassword"];
        AdminPassword = !string.IsNullOrWhiteSpace(configuredPassword) ? configuredPassword : AdminPassword;

        var adminUser = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (adminUser == null)
        {
            var user = new User
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                Surname = "System",
                Abbreviation = "SA",
                Name = "Admin",
            };

            var result = await userManager.CreateAsync(user, AdminPassword);
            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created.");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, Roles.SystemAdministrator))
        {
            await userManager.AddToRoleAsync(adminUser, Roles.SystemAdministrator);
            logger.LogInformation("Admin assigned to SystemAdministrator role.");
        }
    }

    /// <summary>
    /// Seeds school types.
    /// </summary>
    private static async Task SeedSchoolTypes(LisaDbContext dbContext, ILogger logger)
    {
        if (!await dbContext.SchoolTypes.AnyAsync())
        {
            await dbContext.SchoolTypes.AddRangeAsync([
                new SchoolType { Name = "Primary School" },
                new SchoolType { Name = "High School" },
                new SchoolType { Name = "Combined School" }
            ]);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded School Types.");
        }
    }

    /// <summary>
    /// Seeds school curriculums.
    /// </summary>
    private static async Task SeedSchoolCurriculum(LisaDbContext dbContext, ILogger logger)
    {
        if (!await dbContext.SchoolCurriculums.AnyAsync())
        {
            await dbContext.SchoolCurriculums.AddRangeAsync([
                new SchoolCurriculum { Name = "CAPS", Description = "Curriculum Assessment Policy Statements" },
                new SchoolCurriculum { Name = "IEB", Description = "Independent Examinations Board" },
                new SchoolCurriculum { Name = "CIE", Description = "Cambridge International Examinations" }
            ]);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded School Curriculums.");
        }
    }

    private static async Task SeedSytemGrades(LisaDbContext dbContext, ILogger logger)
    {
        if (!await dbContext.SystemGrades.AnyAsync())
        {
            await dbContext.SystemGrades.AddRangeAsync([
                new SystemGrade { SequenceNumber = -2, Name = "Gr RRR"},
                new SystemGrade { SequenceNumber = -1, Name = "Gr RR"},
                new SystemGrade { SequenceNumber = -0, Name = "Gr R"},
                new SystemGrade { SequenceNumber = 1, Name = "Gr 1"},
                new SystemGrade { SequenceNumber = 2, Name = "Gr 2"},
                new SystemGrade { SequenceNumber = 3, Name = "Gr 3"},
                new SystemGrade { SequenceNumber = 4, Name = "Gr 4"},
                new SystemGrade { SequenceNumber = 5, Name = "Gr 5"},
                new SystemGrade { SequenceNumber = 6, Name = "Gr 6"},
                new SystemGrade { SequenceNumber = 7, Name = "Gr 7"},
                new SystemGrade { SequenceNumber = 8, Name = "Gr 8"},
                new SystemGrade { SequenceNumber = 9, Name = "Gr 9"},
                new SystemGrade { SequenceNumber = 10, Name = "Gr 10"},
                new SystemGrade { SequenceNumber = 11, Name = "Gr 11"},
                new SystemGrade { SequenceNumber = 12, Name = "Gr 12"},
            ]);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded System Grades.");
        }
    }


    /// <summary>
    /// Seeds school subjects.
    /// </summary>
    private static async Task SeedSchoolSubjects(LisaDbContext dbContext, ILogger logger)
    {
        if (!await dbContext.Subjects.AnyAsync())
        {
            await dbContext.Subjects.AddRangeAsync([

                new Subject {
                    Id = 1,
                    Name = "English HL (RRR - 12)",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                    Code = "Eng HL",
                    Description = "English Home Language",
                    Order = 1,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 2,
                    Name = "Afrikaans FAL (RRR - 12)",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                    Code = "Afr FAL",
                    Description = "Afrikaans First Additional Language",
                    Order = 2,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 3,
                    Name = "Life Skills (RRR - 6)",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6],
                    Code = "LSK",
                    Description = "Life Skills",
                    Order = 3,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 4,
                    Name = "Life Orientation (7 - 12)",
                    GradesApplicable = [7, 8, 9, 10, 11, 12],
                    Code = "LO",
                    Description = "Life Orientation",
                    Order = 4,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 5,
                    Name = "Mathematics (RRR - 9)",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
                    Code = "MATF",
                    Description = "Mathematics",
                    Order = 5,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 6,
                    Name = "Mathematics (10 - 12)",
                    GradesApplicable = [10, 11, 12],
                    Code = "MATH",
                    Description = "Mathematics",
                    Order = 6,
                    SubjectType = SubjectType.MathCombination
                },

                new Subject {
                    Id = 7,
                    Name = "Mathematical Literacy (10 - 12)",
                    GradesApplicable = [10, 11, 12],
                    Code = "MATL",
                    Description = "Mathematical Literacy",
                    Order = 7,
                    SubjectType = SubjectType.MathCombination
                },

                new Subject {
                    Id = 8,
                    Name = "Natural Sciences (4 - 9)",
                    GradesApplicable = [4, 5, 6, 7, 8, 9],
                    Code = "NS",
                    Description = "Natural Sciences",
                    Order = 8,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 9,
                    Name = "Physical Sciences (10 - 12)",
                    GradesApplicable = [10, 11, 12],
                    Code = "PS",
                    Description = "Physical Sciences",
                    Order = 9,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 10,
                    Name = "Life Sciences (10 - 12)",
                    GradesApplicable = [10, 11, 12],
                    Code = "LSC",
                    Description = "Life Sciences",
                    Order = 10,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 11,
                    Name = "Economic and Management Sciences",
                    GradesApplicable = [7],
                    Code = "EMS",
                    Description = "Economic and Management Sciences",
                    Order = 11,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 11,
                    Name = "EMS - Business",
                    GradesApplicable = [8, 9],
                    Code = "EMSBS",
                    Description = "Economic and Management Sciences - Business",
                    Order = 11,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 12,
                    Name = "EMS - Accounting",
                    GradesApplicable = [8, 9],
                    Code = "EMSAC",
                    Description = "Economic and Management Sciences - Accounting",
                    Order = 12,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 13,
                    Name = "Business Studies",
                    GradesApplicable = [10, 11, 12],
                    Code = "BUS",
                    Description = "Business Studies",
                    Order = 13,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 14,
                    Name = "Accounting",
                    GradesApplicable = [10, 11, 12],
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
            logger.LogInformation("Seeded School Subjects.");
        }
    }
}
