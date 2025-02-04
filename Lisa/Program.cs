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
using Lisa.Components.Pages;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1️⃣ Configure Serilog with Better Error Logging
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
    options.Dsn = builder.Configuration["Sentry:Dsn"]; // Store DSN in configuration
    options.MinimumBreadcrumbLevel = LogLevel.Information;
    options.MinimumEventLevel = LogLevel.Error;
    options.Debug = !builder.Environment.IsProduction();
    options.TracesSampleRate = 1.0;
});

// ✅ 2️⃣ Ensure Detailed Errors Only in Development Mode
builder.Services.AddRazorComponents(options =>
    options.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();

// ✅ 3️⃣ Configure Database Context Factory
builder.Services.AddDbContextFactory<LisaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Lisa")));

// Register JWT authentication.
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // For SignalR, allow the JWT to be passed as a query string parameter.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our SignalR hub, get the token from the query string.
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});


// ✅ 4️⃣ Configure Identity & Security Policies
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

})
.AddEntityFrameworkStores<LisaDbContext>()
.AddDefaultTokenProviders();

// ✅ 5️⃣ Configure Hangfire with Restricted Dashboard
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Lisa"));
    }));
builder.Services.AddHangfireServer();

// 🔹 Restrict dashboard access to Admins only
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2; // 🔹 Limit the number of background workers
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "lisa_auth_token";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);  // 🔹 Extend session duration
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/access-denied";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
});

builder.Services.AddHttpClient();

// ✅ 6️⃣ Register Application Services
builder.Services.AddScoped<CareGroupService>();
builder.Services.AddScoped<CombinationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SchoolGradeService>();
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<RegisterClassService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddScoped<IUiEventService, UiEventService>();
builder.Services.AddScoped<EmailTemplateService>();
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<BugReportService>();
builder.Services.AddScoped<VersionService>();
builder.Services.AddSingleton<IEventBus, EventBus>();
builder.Services.AddScoped<IEventLogRepository, EventLogRepository>();
builder.Services.AddScoped<EmailCampaignService>();
builder.Services.AddScoped<CommunicationService>();
builder.Services.AddSingleton<ILoginStore, InMemoryLoginStore>();
builder.Services.AddSingleton<HangfireAuthorizationFilter>();
builder.Services.AddScoped<SystemGradeService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazorBootstrap();
builder.Services.AddControllers();

var app = builder.Build();

// ✅ 7️⃣ Apply Database Migrations Automatically
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
    dbContext.Database.Migrate(); // 🔹 Ensures migrations are applied
    Log.Information("Database migrated successfully.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database migration failed. Exiting...");
    return;
}

// ✅ 8️⃣ Configure Middleware & Routing
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

// 🔹 Get the registered Hangfire authorization filter
var hangfireAuthFilter = app.Services.GetRequiredService<HangfireAuthorizationFilter>();

// 🔹 Configure Hangfire Dashboard with security best practices
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [hangfireAuthFilter], // 🔥 Use DI to fetch the configured filter
    IsReadOnlyFunc = context =>
    {
        var isProduction = app.Environment.IsProduction();

        // 🔹 Log every dashboard access attempt
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var httpContext = context.GetHttpContext();
        var user = httpContext?.User?.Identity?.Name ?? "Unknown User";

        logger.LogInformation("Hangfire Dashboard accessed by {User}. ReadOnly: {ReadOnly}", user, isProduction);

        return isProduction; // 🔥 Restrict modifications in production
    }
});
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

// ✅ 9️⃣ Handle Application Startup Event Logging
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

// ✅ 1️⃣0️⃣ Run Application Safely
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
