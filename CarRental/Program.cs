using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarRental.Services;
using CarRental.Middleware;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region Logging (Serilog)

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/security.log",
        rollingInterval: RollingInterval.Day,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

#region Database

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

#endregion

#region Identity

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(12); 
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

#endregion

#region Session

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

#endregion


builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<CarAvailabilityService>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddMemoryCache();


var app = builder.Build();

#region Middleware Pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CarRental.Middleware.SessionTimeoutMiddleware>();
app.UseMiddleware<CarRental.Middleware.BruteForceDetectionMiddleware>();


#endregion

#region Seed Demo Data

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
    await DemoDataSeeder.SeedAsync(
        services.GetRequiredService<ApplicationDbContext>());
}

#endregion

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();