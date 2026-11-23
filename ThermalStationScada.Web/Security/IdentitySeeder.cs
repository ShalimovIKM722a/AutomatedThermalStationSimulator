using Microsoft.AspNetCore.Identity;

namespace ThermalStationScada.Web.Security;

public class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        RoleManager<IdentityRole> roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<IdentityUser> userManager =
            services.GetRequiredService<UserManager<IdentityUser>>();
        IConfiguration config =
            services.GetRequiredService<IConfiguration>();

        string[] roles =
            { AppRoles.Admin, AppRoles.Operator, AppRoles.Technician };
        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        string email = config["AdminAccount:Email"]!;
        string password = config["AdminAccount:Password"]!;

        IdentityUser? admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser();
            admin.UserName = email;
            admin.Email = email;
            admin.EmailConfirmed = true;

            IdentityResult result =
                await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Cannot create admin: " +
                    result.Errors.First().Description);
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}
