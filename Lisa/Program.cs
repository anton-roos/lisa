using Lisa.Handlers;
using Lisa.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
builder.Services.AddScoped<CircuitHandler, CustomCircuitHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
