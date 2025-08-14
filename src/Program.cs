using DinkToPdf;
using DinkToPdf.Contracts;
using Lisa.Components;
using Lisa.Data;
using Lisa.Interfaces;
using Lisa.Middleware;
using Lisa.Models.Entities;
using Lisa.Repositories;
using Lisa.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authorization", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10_485_760,
        retainedFileCountLimit: 10,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Logging.AddSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.MinimumBreadcrumbLevel = LogLevel.Information;
    options.MinimumEventLevel = LogLevel.Error;
    options.Debug = !builder.Environment.IsProduction();
    options.TracesSampleRate = 1.0;
});

builder.Services.AddRazorComponents(options =>
        options.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<LisaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Lisa")));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });


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

builder.Services.AddHostedService<BackgroundJobService>();
builder.Services.AddScoped<BackgroundJobService>();

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
builder.Services.AddScoped<IEventLogRepository, EventLogRepository>();
builder.Services.AddSingleton<ILoginStore, InMemoryLoginStore>();
builder.Services.AddScoped<SystemGradeService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmailCampaignService>();
builder.Services.AddScoped<ProgressFeedbackService>();
builder.Services.AddScoped<AssessmentTypeService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AttendanceRecordService>();
builder.Services.AddScoped<TemplateRenderService>();
builder.Services.AddScoped<LeaveEarlyService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazorBootstrap();
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
Log.Information("Database seeding completed successfully.");

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

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
}
finally
{
    Log.CloseAndFlush();
}
