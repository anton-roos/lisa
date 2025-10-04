using DinkToPdf;
using DinkToPdf.Contracts;
using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Pages;
using Lisa.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSeq();

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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "lisa_auth_token";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));
builder.Services.AddAuthorizationBuilder();

builder.Services.AddScoped<CareGroupService>();
builder.Services.AddScoped<CombinationService>();
builder.Services.AddScoped<SchoolGradeService>();
builder.Services.AddScoped<SchoolGradeTimeService>();
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<LearnerService>();
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddMudServices();

builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<LisaDbContext>();

app.Logger.LogInformation("Attempting database migration...");
db.Database.Migrate();
app.Logger.LogInformation("Database migration successful!");

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

app.Run();
