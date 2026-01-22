using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var appsettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "src", "appsettings.json");
if (!File.Exists(appsettingsPath))
{
    appsettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "appsettings.json");
}
builder.Configuration.AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("Lisa");

builder.Services.AddDbContext<LisaDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<LisaDbContext>()
.AddDefaultTokenProviders();

var host = builder.Build();

using var scope = host.Services.CreateScope();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();

// Ensure Teacher role exists
var teacherRoleName = "Teacher";
var teacherRole = await roleManager.FindByNameAsync(teacherRoleName);
if (teacherRole == null)
{
    teacherRole = new IdentityRole<Guid> { Name = teacherRoleName };
    await roleManager.CreateAsync(teacherRole);
    Console.WriteLine($"Created role: {teacherRoleName}");
}

// Get or create a school
var school = await dbContext.Schools.FirstOrDefaultAsync();
if (school == null)
{
    Console.WriteLine("No schools found. Creating a test school...");
    
    // Get or create SchoolType
    var schoolType = await dbContext.SchoolTypes.FirstOrDefaultAsync();
    if (schoolType == null)
    {
        schoolType = new SchoolType { Id = Guid.NewGuid(), Name = "Primary" };
        dbContext.SchoolTypes.Add(schoolType);
        await dbContext.SaveChangesAsync();
    }
    
    // Get or create SchoolCurriculum
    var schoolCurriculum = await dbContext.SchoolCurriculums.FirstOrDefaultAsync();
    if (schoolCurriculum == null)
    {
        schoolCurriculum = new SchoolCurriculum { Id = Guid.NewGuid(), Name = "CAPS", Description = "Curriculum Assessment Policy Statement" };
        dbContext.SchoolCurriculums.Add(schoolCurriculum);
        await dbContext.SaveChangesAsync();
    }
    
    school = new School
    {
        Id = Guid.NewGuid(),
        ShortName = "Test School",
        LongName = "Test School",
        Color = "#1976d2",
        SchoolTypeId = schoolType.Id,
        SchoolCurriculumId = schoolCurriculum.Id
    };
    dbContext.Schools.Add(school);
    await dbContext.SaveChangesAsync();
    Console.WriteLine($"Created test school: {school.LongName} (ID: {school.Id})");
}

// Create teacher user
var teacherEmail = "teacher@test.com";
var teacherPassword = "Teacher123!";
var existingUser = await userManager.FindByEmailAsync(teacherEmail);

if (existingUser != null)
{
    Console.WriteLine($"User with email {teacherEmail} already exists.");
    Console.WriteLine();
    Console.WriteLine("=== EXISTING LOGIN CREDENTIALS ===");
    Console.WriteLine($"Email: {teacherEmail}");
    Console.WriteLine($"Password: {teacherPassword}");
    Console.WriteLine("==================================");
    return;
}

var teacher = new User
{
    Id = Guid.NewGuid(),
    Email = teacherEmail,
    UserName = teacherEmail,
    NormalizedEmail = teacherEmail.ToUpperInvariant(),
    NormalizedUserName = teacherEmail.ToUpperInvariant(),
    Name = "Test",
    Surname = "Teacher",
    Abbreviation = "TT",
    SchoolId = school.Id,
    UserType = "Teacher",
    EmailConfirmed = true,
    SecurityStamp = Guid.NewGuid().ToString(),
    ConcurrencyStamp = Guid.NewGuid().ToString()
};

var result = await userManager.CreateAsync(teacher, teacherPassword);

if (result.Succeeded)
{
    await userManager.AddToRoleAsync(teacher, teacherRoleName);
    Console.WriteLine("✓ Teacher user created successfully!");
    Console.WriteLine();
    Console.WriteLine("=== LOGIN CREDENTIALS ===");
    Console.WriteLine($"Email: {teacherEmail}");
    Console.WriteLine($"Password: {teacherPassword}");
    Console.WriteLine($"Name: {teacher.Name} {teacher.Surname}");
    Console.WriteLine($"School: {school.LongName}");
    Console.WriteLine($"Role: {teacherRoleName}");
    Console.WriteLine("=========================");
}
else
{
    Console.WriteLine("Failed to create teacher user:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  - {error.Description}");
    }
}
