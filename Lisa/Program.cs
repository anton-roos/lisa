using Lisa.Components;
using Lisa.Data;
using Lisa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lisa.Middleware;
using Hangfire;
using Hangfire.PostgreSql;
using Lisa.Models.Entities;
using RazorLight;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Serilog;
using Lisa.Repositories;
using Lisa.Events;

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

builder.Logging.AddSentry(options =>
{
    options.Dsn = "https://3f8a0819c96640c22558018a8c41f3ef@o4508607398150144.ingest.us.sentry.io/4508607411585024"; // Replace with your Sentry DSN
    options.MinimumBreadcrumbLevel = LogLevel.Information; // Log breadcrumbs at Information level or higher
    options.MinimumEventLevel = LogLevel.Error; // Log events at Error level or higher
    options.Debug = true; // Enable debug output (optional, for development)
    options.TracesSampleRate = 1.0; // Enable performance monitoring (0.0 to 1.0)
});

builder.Services.AddSingleton<IRazorLightEngine>(provider =>
{
    return new RazorLightEngineBuilder()
        .UseEmbeddedResourcesProject(typeof(Program))
        .UseMemoryCachingProvider()
        .Build();
});

builder.Services.AddRazorComponents(options =>
    options.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();


// Add Database Context
builder.Services.AddDbContextFactory<LisaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Lisa")));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<LisaDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Lisa"));
    }));
builder.Services.AddHangfireServer();


builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign In settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.Cookie.Name = "lisa_auth_token";
    options.Cookie.MaxAge = TimeSpan.FromHours(6);
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.LoginPath = "/login";
    options.SlidingExpiration = true;
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<CareGroupService>();
builder.Services.AddScoped<CombinationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GradeService>();
builder.Services.AddScoped<LearnerService>();
builder.Services.AddScoped<ParentService>();
builder.Services.AddScoped<RegisterClassService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<TeacherService>();
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

builder.Services.AddHttpContextAccessor();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BlazorAuthMiddleware>();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     await DatabaseSeed.Seed(services);
// }

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Publish(new ApplicationStartedEvent
{
    Environment = app.Environment.EnvironmentName,
});

app.Run();

Log.CloseAndFlush();
