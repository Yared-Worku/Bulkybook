using Bulkybookweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    
    options.Filters.Add(new AuthorizeFilter(policy));
});
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    //if (builder.Environment.IsDevelopment())
//    //{
//    //    // SQL Server for local dev
//    //    options.UseSqlServer(builder.Configuration.GetConnectionString("BULKY_DB"));
//    //}
//    //else
//    //{
//        // Postgres for Render
//        var connUrl = Environment.GetEnvironmentVariable("BULKY_DB");
//        options.UseNpgsql(connUrl);
//    //}
//});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connUrl = builder.Configuration.GetConnectionString("BULKY_DB");

    if (string.IsNullOrEmpty(connUrl))
    {
        throw new InvalidOperationException("Connection string 'BULKY_DB' was not found.");
    }

    if (!connUrl.Contains("SSL Mode"))
    {
        connUrl += ";SSL Mode=Require;Trust Server Certificate=true;";
    }

    options.UseNpgsql(connUrl);
});

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    // We use a fake string just to satisfy the tool
//    options.UseNpgsql("Host=localhost;Database=test;Username=test;Password=test");
//});
builder.Services.AddIdentity<Users, Roles>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
  
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
