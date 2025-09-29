using DinkToPdf;
using DinkToPdf.Contracts;
using Lisa.Components;
using Lisa.Data;
using Lisa.Interfaces;
using Lisa.Middleware;
using Lisa.Models.Entities;
using Lisa.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSeq();
        options.DetailedErrors = builder.Environment.IsDevelopment())

builder.Services.AddDbContextFactory<LisaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Lisa")));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<LisaDbContext>()
    .AddDefaultTokenProviders();

// Configure Data Protection to persist keys and handle multiple instances
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("Lisa.School.Management")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90)); // Keys valid for 90 days

// Configure Antiforgery options for better error handling
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHostedService<BackgroundJobService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "lisa_auth_token";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/access-denied";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

builder.Services.AddScoped<CareGroupService>();
builder.Services.AddScoped<CombinationService>();
builder.Services.AddScoped<SchoolGradeService>();
builder.Services.AddScoped<SchoolGradeTimeService>();
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<ILearnerService, LearnerService>();
builder.Services.AddScoped<RegisterClassService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<UiEventService>();
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<DailyRegisterService>();
builder.Services.AddScoped<AcademicDevelopmentClassService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SystemGradeService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmailCampaignService>();
builder.Services.AddScoped<ProgressFeedbackService>();
builder.Services.AddScoped<AssessmentTypeService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AttendanceRecordService>();
builder.Services.AddScoped<TemplateRenderService>();
builder.Services.AddScoped<LeaveEarlyService>();

// Security
builder.Services.AddSingleton<ILoginStore, InMemoryLoginStore>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddMudServices();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<LisaDbContext>();

app.Logger.LogInformation("Attempting database migration...");
db.Database.Migrate();
app.Logger.LogInformation("Database migration successful!");

var services = scope.ServiceProvider;
await DatabaseSeed.Seed(services);
app.Logger.LogInformation("Database seeding completed successfully.");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BlazorAuthMiddleware>();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
