using Lisa.Components;
using Lisa.Data;
using Lisa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lisa.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Hangfire;
using Hangfire.PostgreSql;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Serilog;
using Lisa.Repositories;
using Lisa.Events;
using Hangfire.Dashboard;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lisa.Models.EmailModels;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
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

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Lisa"));
    }));
builder.Services.AddHangfireServer();

builder.Services.AddHangfireServer(options => { options.WorkerCount = 2; });

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

builder.Services.AddScoped<CareGroupService>();
builder.Services.AddScoped<CombinationService>();
builder.Services.AddScoped<SchoolGradeService>();
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<RegisterClassService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<IUiEventService, UiEventService>();
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<BugReportService>();
builder.Services.AddScoped<VersionService>();
builder.Services.AddSingleton<IEventBus, EventBus>();
builder.Services.AddScoped<IEventLogRepository, EventLogRepository>();
builder.Services.AddSingleton<ILoginStore, InMemoryLoginStore>();
builder.Services.AddSingleton<HangfireAuthorizationFilter>();
builder.Services.AddScoped<SystemGradeService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmailCampaignService>();
builder.Services.AddScoped<EmailRendererService>();
builder.Services.AddTransient<ICampaignTemplateProcessor, ProgressReportCampaignProcessor>();
builder.Services.AddTransient<ICampaignTemplateProcessor, DefaultCampaignProcessor>();
builder.Services.AddScoped<ProgressFeedbackService>();
builder.Services.AddScoped<RazorLightViewToStringRenderer>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazorBootstrap();
builder.Services.AddControllers();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
    dbContext.Database.Migrate();
    Log.Information("Database migrated successfully.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database migration failed. Exiting...");
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BlazorAuthMiddleware>();

var hangfireAuthFilter = app.Services.GetRequiredService<HangfireAuthorizationFilter>();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [hangfireAuthFilter],
    IsReadOnlyFunc = context =>
    {
        var isProduction = app.Environment.IsProduction();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var httpContext = context.GetHttpContext();
        var user = httpContext?.User.Identity?.Name ?? "Unknown User";

        logger.LogInformation("Hangfire Dashboard accessed by {User}. ReadOnly: {ReadOnly}", user, isProduction);

        return isProduction;
    }
});
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await DatabaseSeed.Seed(services);
    Log.Information("Database seeding completed successfully.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error seeding database.");
}

var eventBus = app.Services.GetRequiredService<IEventBus>();
try
{
    await eventBus.PublishAsync(new ApplicationStartedEvent
    {
        Environment = app.Environment.EnvironmentName,
    });
    Log.Information("Application started event published.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error publishing application started event.");
}

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
