using Lisa.Components.Account;
using Lisa.Infrastructure.Data;
using Lisa.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lisa.Web.Components;
using Lisa.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddDbContextFactory<LisaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Lisa")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();


//builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<LisaDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
    app.UseMigrationsEndPoint();
}
else
{
     app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }
