using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.CodeDom.Compiler;
using ThermalStationScada.Web.Controllers;
using ThermalStationScada.Web.Data;
using ThermalStationScada.Web.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ThermalStationScada.Auth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions>(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");

    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();

    options.AddPolicy("CanReadSavings", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddSingleton<LiveValuesCache>();
builder.Services.AddSingleton<SchemeConfigService>();
builder.Services.AddHostedService<MqttListener>();

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
    pattern: "{controller=Scheme}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();