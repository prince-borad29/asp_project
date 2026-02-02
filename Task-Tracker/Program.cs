using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaskTracker.Data;
using TaskTracker.Models;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 1. ADD DATABASE & IDENTITY SERVICES (You were missing these!)
// ====================================================

// Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Identity (Users & Roles)
// IMPORTANT: Use <ApplicationUser, IdentityRole>
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings for simple internal use
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ====================================================

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Run the Seeder (Create Admin if not exists)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error seeding DB: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Authentication (Must be before Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Changed default to Login for safety

app.Run();