using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Data;

public static class DatabaseSeed
{
    private const string DefaultAdminEmail = "admin@email.com";
    private const string DefaultAdminPassword = "TestP@ssword42069";

    public static async Task Seed(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DatabaseSeed));
        logger.LogInformation("Starting database seeding...");

        try
        {
            await using var dbContext = serviceProvider.GetRequiredService<LisaDbContext>();

            await ApplyMigrations(dbContext, logger);

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await SeedRoles(serviceProvider, logger);
            await SeedAdmin(serviceProvider, logger);
            await SeedSchoolTypes(dbContext, logger);
            await SeedSchoolSubjects(dbContext, logger);
            await SeedSchoolCurriculum(dbContext, logger);
            await SeedSytemGrades(dbContext, logger);
            await transaction.CommitAsync();

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

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

    private static async Task SeedRoles(IServiceProvider serviceProvider, ILogger logger)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var roles = new[]
        {
            Roles.SystemAdministrator,
            Roles.Principal,
            Roles.Administrator,
            Roles.SchoolManagement,
            Roles.Teacher,
            Roles.Attendance,
            Roles.Reception
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

        var configuredPassword = DefaultAdminPassword;

        var adminUser = await userManager.FindByEmailAsync(DefaultAdminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                Surname = "System",
                Abbreviation = "SA",
                Name = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, configuredPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Admin user created successfully.");
        }

        if (!await userManager.IsInRoleAsync(adminUser, Roles.SystemAdministrator))
        {
            await userManager.AddToRoleAsync(adminUser, Roles.SystemAdministrator);
            logger.LogInformation("Admin assigned to SystemAdministrator role.");
        }
    }

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
                new SystemGrade { SequenceNumber = -2, Name = "Gr RRR", MathGrade = false, CombinationGrade = false, AchievementLevelRating = true},
                new SystemGrade { SequenceNumber = -1, Name = "Gr RR", MathGrade = false, CombinationGrade = false, AchievementLevelRating = true},
                new SystemGrade { SequenceNumber = -0, Name = "Gr R", MathGrade = false, CombinationGrade = false, AchievementLevelRating = true},
                new SystemGrade { SequenceNumber = 1, Name = "Gr 1", MathGrade = false, CombinationGrade = false, AchievementLevelRating = true},
                new SystemGrade { SequenceNumber = 2, Name = "Gr 2", MathGrade = false, CombinationGrade = false, AchievementLevelRating = true},
                new SystemGrade { SequenceNumber = 3, Name = "Gr 3", MathGrade = false, CombinationGrade = false, AchievementLevelRating = true},
                new SystemGrade { SequenceNumber = 4, Name = "Gr 4", MathGrade = false, CombinationGrade = false, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 5, Name = "Gr 5", MathGrade = false, CombinationGrade = false, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 6, Name = "Gr 6", MathGrade = false, CombinationGrade = false, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 7, Name = "Gr 7", MathGrade = false, CombinationGrade = false,  AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 8, Name = "Gr 8", MathGrade = false, CombinationGrade = false, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 9, Name = "Gr 9", MathGrade = false, CombinationGrade = false, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 10, Name = "Gr 10", MathGrade = true, CombinationGrade = true, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 11, Name = "Gr 11", MathGrade = true, CombinationGrade = true, AchievementLevelRating = false},
                new SystemGrade { SequenceNumber = 12, Name = "Gr 12", MathGrade = true, CombinationGrade = true, AchievementLevelRating = false},
            ]);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded System Grades.");
        }
    }

    private static async Task SeedSchoolSubjects(LisaDbContext dbContext, ILogger logger)
    {
        if (!await dbContext.Subjects.AnyAsync())
        {
            await dbContext.Subjects.AddRangeAsync([

                new Subject {
                    Id = 1,
                    Name = "English HL",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                    Code = "Eng HL",
                    Description = "English Home Language",
                    Order = 1,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 2,
                    Name = "Afrikaans FAL",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                    Code = "Afr FAL",
                    Description = "Afrikaans First Additional Language",
                    Order = 2,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 3,
                    Name = "Life Skills",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6],
                    Code = "LS",
                    Description = "Life Skills",
                    Order = 3,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 4,
                    Name = "Life Orientation",
                    GradesApplicable = [7, 8, 9, 10, 11, 12],
                    Code = "LO",
                    Description = "Life Orientation",
                    Order = 4,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 5,
                    Name = "Mathematics",
                    GradesApplicable = [-2,-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
                    Code = "MA",
                    Description = "Mathematics",
                    Order = 5,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 6,
                    Name = "Mathematics",
                    GradesApplicable = [10, 11, 12],
                    Code = "MAT",
                    Description = "Mathematics",
                    Order = 6,
                    SubjectType = SubjectType.MathCombination
                },

                new Subject {
                    Id = 7,
                    Name = "Mathematical Literacy",
                    GradesApplicable = [10, 11, 12],
                    Code = "ML",
                    Description = "Mathematical Literacy",
                    Order = 7,
                    SubjectType = SubjectType.MathCombination
                },

                new Subject {
                    Id = 8,
                    Name = "Natural Sciences",
                    GradesApplicable = [4, 5, 6, 7, 8, 9],
                    Code = "NS",
                    Description = "Natural Sciences",
                    Order = 8,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 9,
                    Name = "Physical Sciences",
                    GradesApplicable = [10, 11, 12],
                    Code = "PS",
                    Description = "Physical Sciences",
                    Order = 9,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 10,
                    Name = "Life Sciences",
                    GradesApplicable = [10, 11, 12],
                    Code = "LS",
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
                    Id = 12,
                    Name = "EMS - Business",
                    GradesApplicable = [8, 9],
                    Code = "EMSBS",
                    Description = "Economic and Management Sciences - Business",
                    Order = 12,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 13,
                    Name = "EMS - Accounting",
                    GradesApplicable = [8, 9],
                    Code = "EMSAC",
                    Description = "Economic and Management Sciences - Accounting",
                    Order = 13,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 14,
                    Name = "Business Studies",
                    GradesApplicable = [10, 11, 12],
                    Code = "BUS",
                    Description = "Business Studies",
                    Order = 14,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 15,
                    Name = "Accounting",
                    GradesApplicable = [10, 11, 12],
                    Code = "ACC",
                    Description = "Accounting",
                    Order = 15,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 16,
                    Name = "Economics",
                    GradesApplicable = [10, 11, 12],
                    Code = "ECO",
                    Description = "Economics",
                    Order = 16,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 17,
                    Name = "Tourism",
                    GradesApplicable = [10, 11, 12],
                    Code = "TO",
                    Description = "Tourism",
                    Order = 17,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 18,
                    Name = "SS History",
                    GradesApplicable = [4, 5, 6, 7, 8, 9],
                    Code = "SSHIS",
                    Description = "Social Sciences - History",
                    Order = 18,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 19,
                    Name = "SS Geography",
                    GradesApplicable = [4, 5, 6, 7, 8, 9],
                    Code = "SSGEO",
                    Description = "Social Sciences - Geography",
                    Order = 19,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 20,
                    Name = "History",
                    GradesApplicable = [10, 11, 12],
                    Code = "HIS",
                    Description = "History",
                    Order = 20,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 21,
                    Name = "Geography",
                    GradesApplicable = [10, 11, 12],
                    Code = "GEO",
                    Description = "Geography",
                    Order = 21,
                    SubjectType = SubjectType.Combination
                },

                new Subject {
                    Id = 22,
                    Name = "Creative Arts",
                    GradesApplicable = [4, 5, 6, 7, 8, 9],
                    Code = "CA",
                    Description = "Creative Arts",
                    Order = 22,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 23,
                    Name = "Technology",
                    GradesApplicable = [7, 8, 9],
                    Code = "TEC",
                    Description = "Technology",
                    Order = 23,
                    SubjectType = SubjectType.Compulsory
                },

                new Subject {
                    Id = 24,
                    Name = "Computer Applications Technology",
                    GradesApplicable = [10, 11, 12],
                    Code = "CAT",
                    Description = "Computer Applications Technology",
                    Order = 42,
                    SubjectType = SubjectType.Combination
                }
            ]);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded School Subjects.");
        }
    }
}
