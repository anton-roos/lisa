using Lisa.Components;
using Lisa.Data;
using Lisa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Lisa.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Add Database Context
builder.Services.AddDbContext<LisaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Lisa")));

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Identity")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

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

    options.SignIn.RequireConfirmedEmail = false;
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


// Fluent UI Components
builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridEntityFrameworkAdapter();

builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NavigationService>();

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
app.UseMiddleware<BlazorCookieLoginMiddleware>();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
