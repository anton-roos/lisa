using DinkToPdf;
using DinkToPdf.Contracts;
using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.AcademicPlanning;
using Lisa.Pages;
using Lisa.Services.AcademicPlanning;
using Lisa.Services;
using Lisa.Infrastructure.AcademicPlanning;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSeq();

var dbConnectionString = builder.Configuration.GetConnectionString("Lisa");

builder.Services.AddDbContextFactory<LisaDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

// Add DbContext for Identity and services that need it directly (scoped lifetime)
// Using AddDbContextPool for better performance and to avoid scoped service resolution issues
builder.Services.AddDbContextPool<LisaDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

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
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("Lisa.School.Management")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));


builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__RequestVerificationToken";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHostedService<BackgroundJobService>();
builder.Services.AddHostedService<EmailCampaignBackgroundService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "lisa_auth_token";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/denied";
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));
builder.Services.AddAuthorizationBuilder();

builder.Services.AddScoped<AcademicDevelopmentClassService>();
builder.Services.AddScoped<AdiAttendanceService>();
builder.Services.AddScoped<AttendanceRecordService>();
builder.Services.AddScoped<CareGroupService>();
builder.Services.AddScoped<CombinationService>();
builder.Services.AddScoped<SchoolGradeService>();
builder.Services.AddScoped<SchoolGradeTimeService>();
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<LearnerPromotionService>();
builder.Services.AddScoped<RegisterClassService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<UiEventService>();
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SystemGradeService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmailCampaignService>();
builder.Services.AddScoped<ProgressFeedbackService>();
builder.Services.AddScoped<AssessmentTypeService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<TemplateRenderService>();
builder.Services.AddScoped<LeaveEarlyService>();
// Old AcademicPlanningService (for AcademicPlan entities - used by some UI components)
builder.Services.AddScoped<Lisa.Services.AcademicPlanningService>();
// New AcademicPlanningService (for TeachingPlan entities - implements IAcademicPlanningService)
builder.Services.AddScoped<IAcademicPlanningService, Lisa.Services.AcademicPlanning.AcademicPlanningService>();

// Academic Planning Services
builder.Services.AddScoped<Lisa.Services.AcademicPlanning.IAcademicYearSetupService, Lisa.Services.AcademicPlanning.AcademicYearSetupService>();
builder.Services.AddScoped<Lisa.Services.AcademicPlanning.ISubjectGradePeriodService, Lisa.Services.AcademicPlanning.SubjectGradePeriodService>();
builder.Services.AddScoped<Lisa.Services.AcademicPlanning.ITermAssessmentPlanService, Lisa.Services.AcademicPlanning.TermAssessmentPlanService>();
builder.Services.AddScoped<Lisa.Services.AcademicPlanning.IAcademicLibraryService, Lisa.Services.AcademicPlanning.AcademicLibraryService>();
builder.Services.AddScoped<Lisa.Services.AcademicPlanning.IWorkCompletionReportService, Lisa.Services.AcademicPlanning.WorkCompletionReportService>();

// Academic Planning Export Services
builder.Services.AddScoped<AcademicPlanExcelExporter>();
builder.Services.AddScoped<AcademicPlanPdfExporter>();
builder.Services.AddScoped<AcademicPlanExportService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddMudServices();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/login");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/logout");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "/register");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "/denied");
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/Index", "/account");
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Run migrations by creating DbContext directly to avoid scoped service resolution issues
app.Logger.LogInformation("Attempting database migration...");
try
{
    var migrationConnectionString = app.Configuration.GetConnectionString("Lisa");
    var optionsBuilder = new DbContextOptionsBuilder<LisaDbContext>();
    optionsBuilder.UseNpgsql(migrationConnectionString);
    // Suppress pending model changes warning during migration
    optionsBuilder.ConfigureWarnings(warnings => 
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    
    using (var db = new LisaDbContext(optionsBuilder.Options))
    {
        db.Database.Migrate();
    }
    
    app.Logger.LogInformation("Database migration successful!");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database migration failed!");
    throw;
}

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

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapRazorPages();
app.MapControllers();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
    await next();
});

app.Run();
