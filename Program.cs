using kekes.Data;
using kekes.Data.Models;
using kekes.Hubs;
using kekes.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUserPermissionsService, UserPermissionsService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Sections}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapHub<NotificationHub>("/signalServer");

var serviceProvider = builder.Services.BuildServiceProvider();

var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

var adminsRole = await roleManager.FindByNameAsync(ApplicationRoles.Administrators);
if (adminsRole == null)
{
    var roleResult = await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Administrators));
    if (!roleResult.Succeeded)
    {
        throw new InvalidOperationException($"Unable to create {ApplicationRoles.Administrators} role.");
    }

    adminsRole = await roleManager.FindByNameAsync(ApplicationRoles.Administrators);
}

var adminUser = await userManager.FindByNameAsync("admin@localhost.local");
if (adminUser == null)
{
    var userResult = await userManager.CreateAsync(new IdentityUser
    {
        UserName = "admin@localhost.local",
        Email = "admin@localhost.local"
    }, "AdminPass123!");
    if (!userResult.Succeeded)
    {
        throw new InvalidOperationException($"Unable to create admin@localhost.local user");
    }

    adminUser = await userManager.FindByNameAsync("admin@localhost.local");
}

if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Administrators))
{
    await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrators);
}

app.Run();
